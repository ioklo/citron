using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Citron.Infra;
using Citron.Collections;
using Citron.Syntax;
using Citron.Symbol;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;
using System.Diagnostics;

namespace Citron.Analysis;

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

    public static R.Exp? TryCastExp_Exp(R.Exp exp, IType expType, IType expectedType, ScopeContext context) // nothrow
    {
        // 같으면 그대로 리턴
        if (TypeEquals(expectedType, expType))
            return exp;            

        // 1. enumElem -> enum
        if (expType is EnumElemType enumElemType)
        {
            if (expectedType is EnumType expectedEnumType)
            {
                if (TypeEquals(expectedEnumType, enumElemType.GetEnumType()))
                {
                    return new R.CastEnumElemToEnumExp(exp, expectedEnumType.GetSymbol());
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

    public static bool FuncParameterEquals(FuncParameter x, FuncParameter y)
    {
        return TypeEquals(x.Type, y.Type) && x.Name.Equals(y);
    }

    // 값의 겉보기 타입을 변경한다
    public static TranslationResult<R.Exp> CastExp_Exp(R.Exp exp, IType expType, IType expectedType, S.ISyntaxNode nodeForErrorReport, ScopeContext context)
    {
        var result = BodyMisc.TryCastExp_Exp(exp, expType, expectedType, context);
        if (result != null) return TranslationResult.Valid(result);

        context.AddFatalError(A2201_Cast_Failed, nodeForErrorReport);
        return TranslationResult.Error<R.Exp>();
    }

    public static TranslationResult<IR0ExpResult> MakeAsExp(IType targetType, IType testType, R.Exp targetExp)
    {
        TranslationResult<IR0ExpResult> Valid(IR0ExpResult exp) => TranslationResult.Valid(exp);

        // 5가지 케이스로 나뉜다
        if (testType is ClassType testClassType)
        {
            if (targetType is ClassType)
                return Valid(new IR0ExpResult(new R.ClassAsClassExp(targetExp, testClassType), testClassType));
            else if (targetType is InterfaceType)
                return Valid(new IR0ExpResult(new R.InterfaceAsClassExp(targetExp, testClassType), testClassType));
            else
                throw new NotImplementedException(); // 에러 처리
        }
        else if (testType is InterfaceType testInterfaceType)
        {
            if (targetType is ClassType)
                return Valid(new IR0ExpResult(new R.ClassAsInterfaceExp(targetExp, testInterfaceType), testInterfaceType));
            else if (targetType is InterfaceType)
                return Valid(new IR0ExpResult(new R.InterfaceAsInterfaceExp(targetExp, testInterfaceType), testInterfaceType));
            else
                throw new NotImplementedException(); // 에러 처리
        }
        else if (testType is EnumElemType testEnumElemType)
        {
            if (targetType is EnumType)
                return Valid(new IR0ExpResult(new R.EnumAsEnumElemExp(targetExp, testEnumElemType), new NullableType(testEnumElemType)));
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
            return new IntermediateExp.GlobalFuncs(result.OuterAndDeclSymbols, typeArgs);
        }

        IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitClass(SymbolQueryResult.Class result)
        {
            return new IntermediateExp.Class(result.ClassConstructor.Invoke(typeArgs));
        }

        IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitClassMemberFuncs(SymbolQueryResult.ClassMemberFuncs result)
        {
            return new IntermediateExp.ClassMemberFuncs(result.OuterAndDeclSymbols, typeArgs, hasExplicitInstance: false, null);
        }

        IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitClassMemberVar(SymbolQueryResult.ClassMemberVar result)
        {
            return new IntermediateExp.ClassMemberVar(result.Symbol, hasExplicitInstance: false, null);
        }

        IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitStruct(SymbolQueryResult.Struct result)
        {
            return new IntermediateExp.Struct(result.StructConstructor.Invoke(typeArgs));
        }

        IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitStructMemberFuncs(SymbolQueryResult.StructMemberFuncs result)
        {
            return new IntermediateExp.StructMemberFuncs(result.OuterAndDeclSymbols, typeArgs, hasExplicitInstance: false, null);
        }

        IntermediateExp ISymbolQueryResultVisitor<IntermediateExp>.VisitStructMemberVar(SymbolQueryResult.StructMemberVar result)
        {
            return new IntermediateExp.StructMemberVar(result.Symbol, hasExplicitInstance: false, null);
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

    // 어떤 함수를 
    public static TFuncDeclSymbol? GetAccessibleInstanceFunc<TDeclSymbol, TFuncDeclSymbol>(
        IDeclSymbolNode curNode,
        TDeclSymbol declSymbol,
        Name name,
        int typeParamCount,
        ImmutableArray<FuncParamId> paramIds)
        where TDeclSymbol : class
        where TFuncDeclSymbol : class, IFuncDeclSymbol
    {
        // TODO: [12] Extension 함수 검색
        if (declSymbol is not IFuncDeclContainable<TFuncDeclSymbol> funcDeclContainer)
            return null;

        var func = funcDeclContainer.GetFunc(name, typeParamCount, paramIds);
        if (func == null) return null;

        if (!curNode.CanAccess(func)) return null;
        return func;
    }


    struct ASFuncsSQRVisitor : ISymbolQueryResultVisitor<IEnumerable<(ISymbolNode, IFuncDeclSymbol)>>
    {
        static IEnumerable<(ISymbolNode, IFuncDeclSymbol)> Empty() => Enumerable.Empty<(ISymbolNode, IFuncDeclSymbol)>();

        IEnumerable<(ISymbolNode, IFuncDeclSymbol)> ISymbolQueryResultVisitor<IEnumerable<(ISymbolNode, IFuncDeclSymbol)>>.VisitMultipleCandidatesError(SymbolQueryResult.MultipleCandidatesError result)
        {
            return Empty();
        }

        IEnumerable<(ISymbolNode, IFuncDeclSymbol)> ISymbolQueryResultVisitor<IEnumerable<(ISymbolNode, IFuncDeclSymbol)>>.VisitNamespace(SymbolQueryResult.Namespace result)
        {
            return Empty();
        }

        IEnumerable<(ISymbolNode, IFuncDeclSymbol)> ISymbolQueryResultVisitor<IEnumerable<(ISymbolNode, IFuncDeclSymbol)>>.VisitGlobalFuncs(SymbolQueryResult.GlobalFuncs result)
        {
            foreach (var (outer, declSymbol) in result.OuterAndDeclSymbols)
                yield return (outer, declSymbol);
        }

        IEnumerable<(ISymbolNode, IFuncDeclSymbol)> ISymbolQueryResultVisitor<IEnumerable<(ISymbolNode, IFuncDeclSymbol)>>.VisitClass(SymbolQueryResult.Class result)
        {
            return Empty();
        }

        IEnumerable<(ISymbolNode, IFuncDeclSymbol)> ISymbolQueryResultVisitor<IEnumerable<(ISymbolNode, IFuncDeclSymbol)>>.VisitClassMemberFuncs(SymbolQueryResult.ClassMemberFuncs result)
        {
            foreach (var (outer, declSymbol) in result.OuterAndDeclSymbols)
                yield return (outer, declSymbol);
        }

        IEnumerable<(ISymbolNode, IFuncDeclSymbol)> ISymbolQueryResultVisitor<IEnumerable<(ISymbolNode, IFuncDeclSymbol)>>.VisitClassMemberVar(SymbolQueryResult.ClassMemberVar result)
        {
            return Empty();
        }

        IEnumerable<(ISymbolNode, IFuncDeclSymbol)> ISymbolQueryResultVisitor<IEnumerable<(ISymbolNode, IFuncDeclSymbol)>>.VisitStruct(SymbolQueryResult.Struct result)
        {
            return Empty();
        }

        IEnumerable<(ISymbolNode, IFuncDeclSymbol)> ISymbolQueryResultVisitor<IEnumerable<(ISymbolNode, IFuncDeclSymbol)>>.VisitStructMemberFuncs(SymbolQueryResult.StructMemberFuncs result)
        {
            foreach (var (outer, declSymbol) in result.OuterAndDeclSymbols)
                yield return (outer, declSymbol);
        }

        IEnumerable<(ISymbolNode, IFuncDeclSymbol)> ISymbolQueryResultVisitor<IEnumerable<(ISymbolNode, IFuncDeclSymbol)>>.VisitStructMemberVar(SymbolQueryResult.StructMemberVar result)
        {
            return Empty();
        }

        IEnumerable<(ISymbolNode, IFuncDeclSymbol)> ISymbolQueryResultVisitor<IEnumerable<(ISymbolNode, IFuncDeclSymbol)>>.VisitEnum(SymbolQueryResult.Enum result)
        {
            return Empty();
        }

        IEnumerable<(ISymbolNode, IFuncDeclSymbol)> ISymbolQueryResultVisitor<IEnumerable<(ISymbolNode, IFuncDeclSymbol)>>.VisitEnumElem(SymbolQueryResult.EnumElem result)
        {
            return Empty();
        }

        IEnumerable<(ISymbolNode, IFuncDeclSymbol)> ISymbolQueryResultVisitor<IEnumerable<(ISymbolNode, IFuncDeclSymbol)>>.VisitEnumElemMemberVar(SymbolQueryResult.EnumElemMemberVar result)
        {
            return Empty();
        }

        IEnumerable<(ISymbolNode, IFuncDeclSymbol)> ISymbolQueryResultVisitor<IEnumerable<(ISymbolNode, IFuncDeclSymbol)>>.VisitLambdaMemberVar(SymbolQueryResult.LambdaMemberVar result)
        {
            return Empty();
        }

        IEnumerable<(ISymbolNode, IFuncDeclSymbol)> ISymbolQueryResultVisitor<IEnumerable<(ISymbolNode, IFuncDeclSymbol)>>.VisitTupleMemberVar(SymbolQueryResult.TupleMemberVar result)
        {
            return Empty();
        }
    }

    public static IEnumerable<(ISymbolNode Outer, IFuncDeclSymbol DeclSymbol)> AsOuterAndDeclSymbols(this SymbolQueryResult sqr)
    {
        var visitor = new ASFuncsSQRVisitor();
        return sqr.Accept<ASFuncsSQRVisitor, IEnumerable<(ISymbolNode, IFuncDeclSymbol)>>(ref visitor);
    }
}
