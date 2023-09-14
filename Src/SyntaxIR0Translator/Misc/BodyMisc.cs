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
using System.Diagnostics;
using Citron.Syntax;

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

        public static R.Exp? TryCastExp_Exp(R.Exp exp, IType expectedType, ScopeContext context) // nothrow
        {
            var expType = context.GetExpType(exp);

            // 같으면 그대로 리턴
            if (TypeEquals(expectedType, expType))
                return exp;            

            // 1. enumElem -> enum
            if (expType is EnumElemSymbol enumElem)
            {
                if (expectedType is EnumSymbol expectEnum)
                {
                    if (expectedType.Equals(enumElem.GetOuter()))
                    {
                        return new R.CastEnumElemToEnumExp(exp, expectEnum);
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

        public static bool TypeEquals(IType type0, IType type1)
        {
            var typeId0 = type0.GetTypeId();
            var typeId1 = type1.GetTypeId();

            return typeId0.Equals(typeId1);
        }

        // 값의 겉보기 타입을 변경한다
        public static TranslationResult<R.Exp> CastExp_Exp(R.Exp exp, IType expectedType, S.ISyntaxNode nodeForErrorReport, ScopeContext context)
        {
            var result = BodyMisc.TryCastExp_Exp(exp, expectedType, context);
            if (result != null) return TranslationResult.Valid(result);

            context.AddFatalError(A2201_Cast_Failed, nodeForErrorReport);
            return TranslationResult.Error<R.Exp>();
        }

        public static TranslationResult<R.Exp> MakeAsExp(IType targetType, IType testType, R.Exp targetExp)
        {
            TranslationResult<R.Exp> Valid(R.Exp exp) => TranslationResult.Valid(exp);

            // 5가지 케이스로 나뉜다
            if (testType is ClassType testClassType)
            {
                if (targetType is ClassType)
                    return Valid(new R.ClassAsClassExp(targetExp, testClassType));
                else if (targetType is InterfaceType)
                    return Valid(new R.InterfaceAsClassExp(targetExp, testClassType));
                else
                    throw new NotImplementedException(); // 에러 처리
            }
            else if (testType is InterfaceType testInterfaceType)
            {
                if (targetType is ClassType)
                    return Valid(new R.ClassAsInterfaceExp(targetExp, testInterfaceType));
                else if (targetType is InterfaceType)
                    return Valid(new R.InterfaceAsInterfaceExp(targetExp, testInterfaceType));
                else
                    throw new NotImplementedException(); // 에러 처리
            }
            else if (testType is EnumElemType testEnumElemType)
            {
                if (targetType is EnumType)
                    return Valid(new R.EnumAsEnumElemExp(targetExp, testEnumElemType));
                else
                    throw new NotImplementedException(); // 에러 처리
            }
            else
                throw new NotImplementedException(); // 에러 처리
        }

        public static bool IsVarType(TypeExp typeExp)
        {
            return typeExp is IdTypeExp idTypeExp && 
                idTypeExp.Name == "var" && 
                idTypeExp.TypeArgs.Length == 0;
        }

        public struct SymbolQueryResultExpResultTranslator : ISymbolQueryResultVisitor<IntermediateExp>
        {
            ImmutableArray<IType> typeArgs;

            public static IntermediateExp Translate(SymbolQueryResult result, ImmutableArray<IType> typeArgs)
            {
                var builder = new SymbolQueryResultExpResultTranslator() { typeArgs = typeArgs };
                return result.Accept<SymbolQueryResultExpResultTranslator, IntermediateExp>(ref builder);
            }

            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitMultipleCandidatesError(SymbolQueryResult.MultipleCandidatesError result)
            {
                throw new UnreachableException();
            }

            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitNamespace(SymbolQueryResult.Namespace result)
            {
                return new IntermediateExp.Namespace(result.Symbol);
            }

            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitGlobalFuncs(SymbolQueryResult.GlobalFuncs result)
            {
                return new IntermediateExp.GlobalFuncs(result.Infos, typeArgs);
            }

            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitClass(SymbolQueryResult.Class result)
            {
                return new IntermediateExp.Class(result.ClassConstructor.Invoke(typeArgs));
            }

            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitClassMemberFuncs(SymbolQueryResult.ClassMemberFuncs result)
            {
                return new IntermediateExp.ClassMemberFuncs(result.Infos, typeArgs, HasExplicitInstance: false, null);
            }

            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitClassMemberVar(SymbolQueryResult.ClassMemberVar result)
            {
                return new IntermediateExp.ClassMemberVar(result.Symbol, HasExplicitInstance: false, null);
            }

            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitStruct(SymbolQueryResult.Struct result)
            {
                return new IntermediateExp.Struct(result.StructConstructor.Invoke(typeArgs));
            }

            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitStructMemberFuncs(SymbolQueryResult.StructMemberFuncs result)
            {
                return new IntermediateExp.StructMemberFuncs(result.Infos, typeArgs, HasExplicitInstance: false, null);
            }

            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitStructMemberVar(SymbolQueryResult.StructMemberVar result)
            {
                return new IntermediateExp.StructMemberVar(result.Symbol, HasExplicitInstance: false, null);
            }

            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitEnum(SymbolQueryResult.Enum result)
            {
                return new IntermediateExp.Enum(result.EnumConstructor.Invoke(typeArgs));
            }

            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitEnumElem(SymbolQueryResult.EnumElem result)
            {
                return new IntermediateExp.EnumElem(result.Symbol);
            }

            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitEnumElemMemberVar(SymbolQueryResult.EnumElemMemberVar result)
            {
                // EnumElem에 대하여 멤버 함수를 만들 수 없으므로, Identifier로는 지칭할 수 없다
                throw new RuntimeFatalException();
            }

            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitLambdaMemberVar(SymbolQueryResult.LambdaMemberVar result)
            {
                return new IntermediateExp.LambdaMemberVar(result.Symbol);
            }

            IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitTupleMemberVar(SymbolQueryResult.TupleMemberVar result)
            {
                // Tuple에 대하여 멤버 함수를 만들 수 없으므로, Identifier로는 지칭할 수 없다
                throw new RuntimeFatalException();
            }
        }
    }
}
