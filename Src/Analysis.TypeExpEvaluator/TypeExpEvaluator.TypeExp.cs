using System.Collections.Generic;
using System.Text;
using Citron.Infra;

using Citron.Syntax;
using Citron.Module;
using System.Linq;
using Citron.Collections;

using static Citron.Analysis.TypeExpErrorCode;
using Citron.Symbol;

namespace Citron.Analysis
{
    public partial class TypeExpEvaluator
    {
        struct TypeExpVisitor
        {
            TypeEnv typeEnv;
            Context context;

            public TypeExpVisitor(TypeEnv typeEnv, Context context)
            {
                this.typeEnv = typeEnv;
                this.context = context;
            }

            public static void Visit(TypeExp exp, TypeEnv typeEnv, Context context)
            {
                var visitor = new TypeExpVisitor(typeEnv, context);
                visitor.VisitTypeExpOuterMost(exp);
            }

            public static void Visit(ImmutableArray<TypeExp> typeArgExps, TypeEnv typeEnv, Context context)
            {
                var visitor = new TypeExpVisitor(typeEnv, context);

                foreach (var typeArgExp in typeArgExps)
                    visitor.VisitTypeExpOuterMost(typeArgExp);
            }            

            TypeExpInfo VisitIdTypeExp(IdTypeExp typeExp)
            {
                if (typeExp.Name == "var")
                {
                    if (typeExp.TypeArgs.Length != 0)
                        context.Throw(T0102_IdTypeExp_VarTypeCantApplyTypeArgs, typeExp, "var는 타입 인자를 가질 수 없습니다");

                    return SpecialTypeExpInfo.Var(typeExp);
                }
                else if (typeExp.Name == "void")
                {
                    if (typeExp.TypeArgs.Length != 0)
                        context.Throw(T0101_IdTypeExp_TypeDoesntHaveTypeParams, typeExp, "void는 타입 인자를 가질 수 없습니다");

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

                // 1. TypeVar에서 먼저 검색
                var typeVar = typeEnv.TryMakeTypeVar(typeExp.Name, typeExp);
                if (typeVar != null)
                {
                    if (typeExp.TypeArgs.Length != 0)
                        context.Throw(T0105_IdTypeExp_TypeVarCantApplyTypeArgs, typeExp, "타입 변수는 타입 인자를 가질 수 없습니다");

                    return typeVar;
                }

                // TODO: 2. 현재 This Context에서 검색

                // 3. 전역에서 검색, 
                // TODO: 현재 namespace 상황에 따라서 Namespace.Root대신 인자를 집어넣어야 한다.
                var typeArgs = VisitTypeArgExps(typeExp.TypeArgs);
                var candidates = context.GetTypeExpInfos(new SymbolPath(null, new Name.Normal(typeExp.Name), typeArgs), typeExp).ToList();

                if (candidates.Count == 1)
                {
                    return candidates[0];
                }
                else if (1 < candidates.Count)
                {
                    context.Throw(T0103_IdTypeExp_MultipleTypesOfSameName, typeExp, $"이름이 같은 {typeExp} 타입이 여러개 입니다");
                }
                else
                {
                    context.Throw(T0104_IdTypeExp_TypeNotFound, typeExp, $"{typeExp}를 찾지 못했습니다");
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

                var memberResult = parentResult.GetMemberInfo(exp.MemberName, typeArgs, exp);
                if (memberResult == null)
                    context.Throw(T0202_MemberTypeExp_MemberTypeNotFound, exp, $"{parentResult}에서 {exp.MemberName}을 찾을 수 없습니다");

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
                    context.AddInfo(exp, result);
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
