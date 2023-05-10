using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Citron.Infra;
using Citron.Collections;
using Citron.Symbol;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis
{
    static class BodyMisc
    {
        public static ImmutableArray<IType> MakeTypeArgs(ImmutableArray<S.TypeExp> typeArgsSyntax, ScopeContext context)
        {
            var builder = ImmutableArray.CreateBuilder<IType>(typeArgsSyntax.Length);

            foreach (var typeExp in typeArgsSyntax)
            {
                var typeValue = context.MakeType(typeExp);
                builder.Add(typeValue);
            }

            return builder.MoveToImmutable();
        }

        public static R.Exp? TryCastExp_Exp(R.Exp exp, IType expectedType) // nothrow
        {
            var expType = exp.GetExpType();

            // 같으면 그대로 리턴
            if (expectedType.Equals(expType))
                return exp;            

            // 1. enumElem -> enum
            if (expType is EnumElemSymbol enumElem)
            {
                if (expectedType is EnumSymbol expectEnumType)
                {
                    if (expectedType.Equals(enumElem.GetOuter()))
                    {
                        return new R.CastEnumElemToEnumExp(exp, enumElem);
                    }
                }

                return null;
            }

            // 2. exp is class type
            if (expType is ClassSymbol @class)
            {
                if (expectedType is ClassSymbol expectedClass)
                {
                    // allows upcast
                    if (expectedClass.IsBaseOf(@class))
                    {
                        return new R.CastClassExp(exp, expectedClass);
                    }

                    return null;
                }

                // TODO: interface
                // if (expectType is InterfaceTypeValue )
            }

            // TODO: 3. C -> Nullable<C>, C -> B -> Nullable<B> 허용
            //if (IsNullableType(expectedType, out var expectedInnerType))
            //{
            //    // C -> B 시도
            //    var castToInnerTypeExp = TryCastExp_Exp(exp, expectedInnerType);
            //    if (castToInnerTypeExp != null)
            //    {
            //        // B -> B?
            //        return MakeNullableExp(castToInnerTypeExp);
            //        return new R.NewNullableExp(castToInnerTypeExp, expectedNullableType);
            //    }
            //}

            return null;
        }

        // 값의 겉보기 타입을 변경한다
        public static R.Exp CastExp_Exp(R.Exp exp, IType expectedType, S.ISyntaxNode nodeForErrorReport, ScopeContext context) // throws AnalyzeFatalException
        {
            var result = BodyMisc.TryCastExp_Exp(exp, expectedType);
            if (result != null) return result;

            context.AddFatalError(A2201_Cast_Failed, nodeForErrorReport);
            return Error();
        }

        public struct SymbolQueryResultExpResultTranslator : ISymbolQueryResultVisitor<ExpResult>
        {
            ImmutableArray<IType> typeArgs;

            public static ExpResult Translate(SymbolQueryResult result, ImmutableArray<IType> typeArgs)
            {
                var builder = new SymbolQueryResultExpResultTranslator() { typeArgs = typeArgs };
                return result.Accept<SymbolQueryResultExpResultTranslator, ExpResult>(ref builder);
            }

            ExpResult ISymbolQueryResultVisitor<ExpResult>.VisitNotFound(SymbolQueryResult.NotFound result)
            {
                return ExpResults.NotFound;
            }

            ExpResult ISymbolQueryResultVisitor<ExpResult>.VisitMultipleCandidatesError(SymbolQueryResult.MultipleCandidatesError result)
            {
                return ExpResults.MultipleCandiates;
            }

            ExpResult ISymbolQueryResultVisitor<ExpResult>.VisitNamespace(SymbolQueryResult.Namespace result)
            {
                return new ExpResult.Namespace(result.Symbol);
            }

            ExpResult ISymbolQueryResultVisitor<ExpResult>.VisitGlobalFuncs(SymbolQueryResult.GlobalFuncs result)
            {
                return new ExpResult.GlobalFuncs(result.Infos, typeArgs);
            }

            ExpResult ISymbolQueryResultVisitor<ExpResult>.VisitClass(SymbolQueryResult.Class result)
            {
                return new ExpResult.Class(result.ClassConstructor.Invoke(typeArgs));
            }

            ExpResult ISymbolQueryResultVisitor<ExpResult>.VisitClassMemberFuncs(SymbolQueryResult.ClassMemberFuncs result)
            {
                return new ExpResult.ClassMemberFuncs(result.Infos, typeArgs, HasExplicitInstance: false, null);
            }

            ExpResult ISymbolQueryResultVisitor<ExpResult>.VisitClassMemberVar(SymbolQueryResult.ClassMemberVar result)
            {
                return new ExpResult.ClassMemberVar(result.Symbol, HasExplicitInstance: false, null);
            }

            ExpResult ISymbolQueryResultVisitor<ExpResult>.VisitStruct(SymbolQueryResult.Struct result)
            {
                return new ExpResult.Struct(result.StructConstructor.Invoke(typeArgs));
            }

            ExpResult ISymbolQueryResultVisitor<ExpResult>.VisitStructMemberFuncs(SymbolQueryResult.StructMemberFuncs result)
            {
                return new ExpResult.StructMemberFuncs(result.Infos, typeArgs, HasExplicitInstance: false, null);
            }

            ExpResult ISymbolQueryResultVisitor<ExpResult>.VisitStructMemberVar(SymbolQueryResult.StructMemberVar result)
            {
                return new ExpResult.StructMemberVar(result.Symbol, HasExplicitInstance: false, null);
            }

            ExpResult ISymbolQueryResultVisitor<ExpResult>.VisitEnum(SymbolQueryResult.Enum result)
            {
                return new ExpResult.Enum(result.EnumConstructor.Invoke(typeArgs));
            }

            ExpResult ISymbolQueryResultVisitor<ExpResult>.VisitEnumElem(SymbolQueryResult.EnumElem result)
            {
                return new ExpResult.EnumElem(result.Symbol);
            }

            ExpResult ISymbolQueryResultVisitor<ExpResult>.VisitEnumElemMemberVar(SymbolQueryResult.EnumElemMemberVar result)
            {
                // EnumElem에 대하여 멤버 함수를 만들 수 없으므로, Identifier로는 지칭할 수 없다
                throw new RuntimeFatalException();
            }

            ExpResult ISymbolQueryResultVisitor<ExpResult>.VisitLambdaMemberVar(SymbolQueryResult.LambdaMemberVar result)
            {
                return new ExpResult.LambdaMemberVar(result.Symbol);
            }

            ExpResult ISymbolQueryResultVisitor<ExpResult>.VisitTupleMemberVar(SymbolQueryResult.TupleMemberVar result)
            {
                // Tuple에 대하여 멤버 함수를 만들 수 없으므로, Identifier로는 지칭할 수 없다
                throw new RuntimeFatalException();
            }
        }
    }
}
