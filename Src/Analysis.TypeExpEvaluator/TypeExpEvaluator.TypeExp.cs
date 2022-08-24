using System.Collections.Generic;
using System.Text;
using System.Linq;

using Citron.Infra;
using Citron.Syntax;
using Citron.Module;
using Citron.Symbol;
using Citron.Collections;

using static Citron.Analysis.TypeExpErrorCode;

namespace Citron.Analysis
{
    public partial class TypeExpEvaluator
    {
        struct TypeExpVisitor
        {
            LocalContext localContext;
            GlobalContext globalContext;

            public TypeExpVisitor(LocalContext localContext, GlobalContext globalContext)
            {
                this.localContext = localContext;
                this.globalContext = globalContext;
            }

            public static void Visit(TypeExp exp, LocalContext localContext, GlobalContext globalContext)
            {
                var visitor = new TypeExpVisitor(localContext, globalContext);
                visitor.VisitTypeExpOuterMost(exp);
            }

            public static void Visit(ImmutableArray<TypeExp> typeArgExps, LocalContext localContext, GlobalContext globalContext)
            {
                var visitor = new TypeExpVisitor(localContext, globalContext);

                foreach (var typeArgExp in typeArgExps)
                    visitor.VisitTypeExpOuterMost(typeArgExp);
            }            

            TypeExpInfo VisitIdTypeExp(IdTypeExp typeExp)
            {
                if (typeExp.Name == "var")
                {
                    if (typeExp.TypeArgs.Length != 0)
                        globalContext.Throw(T0102_IdTypeExp_VarTypeCantApplyTypeArgs, typeExp, "var는 타입 인자를 가질 수 없습니다");

                    return SpecialTypeExpInfo.Var(typeExp);
                }
                else if (typeExp.Name == "void")
                {
                    if (typeExp.TypeArgs.Length != 0)
                        globalContext.Throw(T0101_IdTypeExp_TypeDoesntHaveTypeParams, typeExp, "void는 타입 인자를 가질 수 없습니다");

                    return SpecialTypeExpInfo.Void(typeExp);
                }

                // built-in
                else if (typeExp.Name == "bool")
                {
                    return BuiltinTypeExpInfo.Bool(typeExp);
                }
                else if (typeExp.Name == "int")
                {
                    return BuiltinTypeExpInfo.Int(typeExp);
                }
                else if (typeExp.Name == "string")
                {
                    return BuiltinTypeExpInfo.String(typeExp);
                }

                // 다시 짜보자


                // 1. TypeVar에서 먼저 검색
                // 
                //var typeVar = typeEnv.TryMakeTypeVar(typeExp.Name, typeExp);
                //if (typeVar != null)
                //{
                //    if (typeExp.TypeArgs.Length != 0)
                //        context.Throw(T0105_IdTypeExp_TypeVarCantApplyTypeArgs, typeExp, "타입 변수는 타입 인자를 가질 수 없습니다");
                //    return typeVar;
                //}
                // => this context에서 검색하면 되도록 수정되었다

                var typeArgs = VisitTypeArgExps(typeExp.TypeArgs);

                // 2. 현재 TypeEnv에서 검색
                // class C<T, U> { struct S<T, U> { void F<X>() { 'S<T, X>' s; } }
                // typeEnv.MakeInternalTypeExpInfo(new SymbolPath(outer: ))
                var localTypeExpInfo = localContext.MakeTypeExpInfo(typeExp.Name, typeArgs.Length, typeExp);
                if (localTypeExpInfo != null)
                    return localTypeExpInfo;

                // 3. 전역에서 검색, 
                // TODO: 현재 namespace 상황에 따라서 outer에 null대신 인자를 집어넣어야 한다.                
                var candidates = globalContext.MakeCandidates(new SymbolPath(outer: null, new Name.Normal(typeExp.Name), typeArgs));
                var candidate = candidates.GetSingle();

                if (candidate != null)
                {
                    return candidate.Invoke(typeExp);
                }
                else if (candidates.HasMultiple)
                {
                    globalContext.Throw(T0103_IdTypeExp_MultipleTypesOfSameName, typeExp, $"이름이 같은 {typeExp} 타입이 여러개 입니다");
                }
                else if (candidates.IsEmpty)
                {
                    globalContext.Throw(T0104_IdTypeExp_TypeNotFound, typeExp, $"{typeExp}를 찾지 못했습니다");
                }

                throw new UnreachableCodeException();
            }

            // X<T>.Y<U, V>
            TypeExpInfo VisitMemberTypeExp(MemberTypeExp exp)
            {
                // X<T>
                var parentResult = VisitTypeExp(exp.Parent);

                // U, V            
                var typeArgs = VisitTypeArgExps(exp.TypeArgs);

                var memberResult = parentResult.MakeMemberInfo(exp.MemberName, typeArgs, exp);
                if (memberResult == null)
                    globalContext.Throw(T0202_MemberTypeExp_MemberTypeNotFound, exp, $"{parentResult}에서 {exp.MemberName}을 찾을 수 없습니다");

                return memberResult;
            }

            // int?
            TypeExpInfo VisitNullableTypeExp(NullableTypeExp exp)
            {
                // int
                var innerTypeResult = VisitTypeExp(exp.InnerTypeExp);
                var typeId = innerTypeResult.GetSymbolId();                

                return SpecialTypeExpInfo.Nullable(typeId, exp);
            }

            TypeExpInfo VisitTypeExp(TypeExp exp)
            {
                switch (exp)
                {
                    case IdTypeExp idExp: return VisitIdTypeExp(idExp);
                    case MemberTypeExp memberExp: return VisitMemberTypeExp(memberExp);
                    case NullableTypeExp nullableExp: return VisitNullableTypeExp(nullableExp);

                    default: throw new UnreachableCodeException();
                }
            }

            // VisitTypeExp와 다른 점은 실행 후 (TypeExp => TypeExpInfo) 정보를 추가
            void VisitTypeExpOuterMost(TypeExp exp)
            {
                try
                {
                    var result = VisitTypeExp(exp);
                    globalContext.AddInfo(exp, result);
                }
                catch (FatalException)
                {

                }
            }

            ImmutableArray<SymbolId> VisitTypeArgExps(ImmutableArray<TypeExp> typeArgExps)
            {
                var builder = ImmutableArray.CreateBuilder<SymbolId>(typeArgExps.Length);
                foreach (var typeArgExp in typeArgExps)
                {
                    var typeArgResult = VisitTypeExp(typeArgExp);
                    var typeArg = typeArgResult.GetSymbolId();

                    builder.Add(typeArg);
                }

                return builder.MoveToImmutable();
            }
        }
    }
}
