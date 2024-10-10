#include "pch.h"
#include "ImCallableAndSArgsToRExpBinding.h"

namespace Citron::SyntaxIR0Translator {

//interface IFuncs<TFuncDeclSymbol, TFuncSymbol>
//where TFuncDeclSymbol : IFuncDeclSymbol
//where TFuncSymbol : IFuncSymbol
//{
//    int GetCount();
//    TFuncDeclSymbol GetDecl(int i);
//
//    TypeEnv GetOuterTypeEnv(int i);
//    ImmutableArray<IType> GetPartialTypeArgs();
//    TFuncSymbol MakeSymbol(int i, ImmutableArray<IType> typeArgs, ScopeContext context);
//}
//
//    // (IntermediateExp, Args) -> TranslationResult<IR0ExpResult>
//partial struct CallableAndArgsBinder : IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>
//{
//    ImmutableArray<S.Argument> argSyntaxes;
//    ScopeContext context;
//
//    S.ISyntaxNode nodeForCallExpErrorReport;
//    S.ISyntaxNode nodeForCallableErrorReport;
//
//    public static TranslationResult<IR0ExpResult> Bind(IntermediateExp callable, ImmutableArray<S.Argument> argSyntaxes, ScopeContext context, S.ISyntaxNode nodeForCallExpErrorReport, S.ISyntaxNode nodeForCallableErrorReport)
//    {
//        // 여기서 분석해야 할 것은 
//        // 1. 해당 Exp가 함수인지, 변수인지, 함수라면 FuncId를 넣어준다
//        // 2. Callable 인자에 맞게 잘 들어갔는지 -> 완료
//        // 3. 잘 들어갔다면 리턴타입 -> 완료
//
//        // TODO: 함수 이름을 먼저 찾고, 타입 힌트에 따라서 Exp를 맞춰봐야 한다
//        // 함수 이름을 먼저 찾는가
//        // Argument 타입을 먼저 알아내야 하는가
//        // F(First); F(E.First); 가 되게 하려면 이름으로 먼저 찾고, 인자타입을 맞춰봐야 한다
//
//        var binder = new CallableAndArgsBinder() { argSyntaxes = argSyntaxes, context = context, nodeForCallableErrorReport = nodeForCallableErrorReport, nodeForCallExpErrorReport = nodeForCallExpErrorReport };
//        return callable.Accept<CallableAndArgsBinder, TranslationResult<IR0ExpResult>>(ref binder);
//    }
//
//    TranslationResult<IR0ExpResult> FatalCallable(SyntaxAnalysisErrorCode code)
//    {
//        context.AddFatalError(code, nodeForCallableErrorReport);
//        return TranslationResult.Error<IR0ExpResult>();
//    }
//
//    TranslationResult<IR0ExpResult> FatalCallExp(SyntaxAnalysisErrorCode code)
//    {
//        context.AddFatalError(code, nodeForCallExpErrorReport);
//        return TranslationResult.Error<IR0ExpResult>();
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitClass(IntermediateExp.Class imExp)
//    {
//        return FatalCallable(A0902_CallExp_CallableExpressionIsNotCallable);
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitClassMemberFuncs(IntermediateExp.ClassMemberFuncs imExp)
//    {
//        return VisitClassMemberFuncs(imExp);
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitClassMemberVar(IntermediateExp.ClassMemberVar imExp)
//    {
//        return HandleLoc(imExp);
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitEnum(IntermediateExp.Enum imExp)
//    {
//        return FatalCallable(A0902_CallExp_CallableExpressionIsNotCallable);
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitEnumElem(IntermediateExp.EnumElem imExp)
//    {
//        // callable이 타입으로 계산되면 Struct과 EnumElem의 경우 생성자 호출을 한다
//        return VisitEnumElem(imExp.Symbol);
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitEnumElemMemberVar(IntermediateExp.EnumElemMemberVar imExp)
//    {
//        return HandleLoc(imExp);
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitGlobalFuncs(IntermediateExp.GlobalFuncs imExp)
//    {
//        return VisitGlobalFuncs(imExp);
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitIR0Exp(IntermediateExp.IR0Exp imExp)
//    {
//        return HandleLoc(imExp);
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitLambdaMemberVar(IntermediateExp.LambdaMemberVar imExp)
//    {
//        return HandleLoc(imExp);
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitLocalVar(IntermediateExp.LocalVar imExp)
//    {
//        return HandleLoc(imExp);
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitNamespace(IntermediateExp.Namespace imExp)
//    {
//        return FatalCallable(A0902_CallExp_CallableExpressionIsNotCallable);
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitStruct(IntermediateExp.Struct imExp)
//    {
//        // callable이 타입으로 계산되면 Struct과 EnumElem의 경우 생성자 호출을 한다
//        return VisitStruct(imExp.Symbol);
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitStructMemberFuncs(IntermediateExp.StructMemberFuncs imExp)
//    {
//        return VisitStructMemberFuncs(imExp);
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitStructMemberVar(IntermediateExp.StructMemberVar imExp)
//    {
//        return HandleLoc(imExp);
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitThis(IntermediateExp.ThisVar imExp)
//    {
//        return FatalCallable(A0902_CallExp_CallableExpressionIsNotCallable);
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitTypeVar(IntermediateExp.TypeVar imExp)
//    {
//        return FatalCallable(A0902_CallExp_CallableExpressionIsNotCallable);
//    }
//
//    // l[0]
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitListIndexer(IntermediateExp.ListIndexer imExp)
//    {
//        return HandleLoc(imExp);
//    }
//
//    TranslationResult<IR0ExpResult> Error()
//    {
//        return TranslationResult.Error<IR0ExpResult>();
//    }
//
//    TranslationResult<IR0ExpResult> Valid(IR0ExpResult expResult)
//    {
//        return TranslationResult.Valid(expResult);
//    }
//
//    // CallExp분석에서 Callable이 GlobalMemberFuncs인 경우 처리
//    TranslationResult<IR0ExpResult> VisitGlobalFuncs(IntermediateExp.GlobalFuncs funcs)
//    {
//        var matchResult = FuncsMatcher.Match(funcs, argSyntaxes, context);
//        if (matchResult == null)
//            throw new NotImplementedException(); // 매치에 실패했습니다.
//
//        var(func, args) = matchResult.Value;
//        return Valid(new IR0ExpResult(new R.CallGlobalFuncExp(func, args), func.GetReturn().Type));
//    }
//
//    // CallExp분석에서 Callable이 ClassMemberFuncs인 경우 처리
//    TranslationResult<IR0ExpResult> VisitClassMemberFuncs(IntermediateExp.ClassMemberFuncs funcs)
//    {
//        var matchResult = FuncsMatcher.Match(funcs, argSyntaxes, context);
//        if (matchResult == null)
//            throw new NotImplementedException(); // 매치에 실패했습니다.
//
//        var(func, args) = matchResult.Value;
//
//        if (funcs.HasExplicitInstance) // x.F, C.F 등 인스턴스 부분이 명시적으로 정해졌다면
//        {
//            // static함수를 인스턴스를 통해 접근하려고 했을 경우 에러 처리
//            if (func.IsStatic() && funcs.ExplicitInstance != null)
//                return FatalCallable(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance);
//
//            // 인스턴스 함수를 인스턴스 없이 호출하려고 했다면
//            if (!func.IsStatic() && funcs.ExplicitInstance == null)
//                return FatalCallable(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType);
//
//            // ResolvedExp -> RExp
//            R.Loc ? instance = null;
//
//            if (funcs.ExplicitInstance != null)
//            {
//                var instResult = ResolvedExpIR0LocTranslator.Translate(
//                    funcs.ExplicitInstance,
//                    context,
//                    bWrapExpAsLoc: true,
//                    nodeForCallableErrorReport,
//                    A2015_ResolveIdentifier_ExpressionIsNotLocation);
//
//                if (!instResult.IsValid(out var inst))
//                    return Error();
//
//                instance = inst.Loc;
//            }
//
//            return Valid(new IR0ExpResult(new R.CallClassMemberFuncExp(func, instance, args), func.GetReturn().Type));
//        }
//        else // F 로 인스턴스를 명시적으로 정하지 않았다면 
//        {
//            if (func.IsStatic()) // 정적함수이면 인스턴스에 null
//            {
//                return Valid(new IR0ExpResult(new R.CallClassMemberFuncExp(func, null, args), func.GetReturn().Type));
//            }
//            else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
//            {
//                return Valid(new IR0ExpResult(new R.CallClassMemberFuncExp(func, new R.ThisLoc(), args), func.GetReturn().Type));
//            }
//        }
//
//        //if (func.IsSequence)
//        //{
//        //    // TODO: funcValue.RetType을 쓰면 의미가 와닿지 않는데, 쉽게 실수 할 수 있을 것 같다
//        //    var seqTypeValue = context.GetSeqTypeValue(funcValue.MakeRPath(), funcValue.GetRetType());
//        //    return new IntermediateExp.Exp(new R.CallSeqFuncExp(funcValue.MakeRPath(), funcsResult.Instance, matchedFunc.Args), seqTypeValue);
//        //}
//        //else
//        //{
//        //    return new IntermediateExp.Exp(new R.CallClassMemberFuncExp(func, funcs.Instance, args));
//        //}
//    }
//
//    // CallExp분석에서 Callable이 StructMemberFuncs인 경우 처리
//    TranslationResult<IR0ExpResult> VisitStructMemberFuncs(IntermediateExp.StructMemberFuncs funcs)
//    {
//        var matchResult = FuncsMatcher.Match(funcs, argSyntaxes, context);
//        if (matchResult == null)
//            throw new NotImplementedException(); // 매치에 실패했습니다.
//
//        var(func, args) = matchResult.Value;
//
//        // static 함수를 호출하는 위치가 선언한 타입 내부라면 체크하지 않고 넘어간다 (멤버 호출이 아닌 경우)
//        if (funcs.HasExplicitInstance)
//        {
//            // static this 체크
//            if (func.IsStatic() && funcs.ExplicitInstance != null)
//                return FatalCallable(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance);
//
//            // 반대의 경우도 체크
//            if (!func.IsStatic() && funcs.ExplicitInstance == null)
//                return FatalCallable(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType);
//
//            R.Loc ? instance = null;
//            if (funcs.ExplicitInstance != null)
//            {
//                var instResult = ResolvedExpIR0LocTranslator.Translate(
//                    funcs.ExplicitInstance,
//                    context,
//                    bWrapExpAsLoc: true,
//                    nodeForCallableErrorReport,
//                    A2015_ResolveIdentifier_ExpressionIsNotLocation);
//
//                if (!instResult.IsValid(out var inst))
//                    return Error();
//
//                instance = inst.Loc;
//            }
//
//            return Valid(new IR0ExpResult(new R.CallStructMemberFuncExp(func, instance, args), func.GetReturn().Type));
//        }
//        else
//        {
//            if (func.IsStatic()) // 정적함수이면 인스턴스에 null
//            {
//                return Valid(new IR0ExpResult(new R.CallStructMemberFuncExp(func, null, args), func.GetReturn().Type));
//            }
//            else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
//            {
//                var thisLoc = new R.ThisLoc();
//                return Valid(new IR0ExpResult(new R.CallStructMemberFuncExp(func, thisLoc, args), func.GetReturn().Type));
//            }
//        }
//
//        //if (func.IsSequence)
//        //{
//        //    // TODO: funcValue.RetType을 쓰면 의미가 와닿지 않는데, 쉽게 실수 할 수 있을 것 같다
//        //    var seqTypeValue = context.GetSeqTypeValue(funcValue.MakeRPath(), funcValue.GetRetType());
//        //    return new IntermediateExp.Exp(new R.CallSeqFuncExp(funcValue.MakeRPath(), funcsResult.Instance, matchedFunc.Args), seqTypeValue);
//        //}
//        //else
//        //{
//        //    return new IntermediateExp.Exp(new R.CallStructMemberFuncExp(func, funcs.ExplicitInstance, args));
//        //}
//    }
//
//    // CallExp 분석에서 Callable이 Lambda, func<>로 계산되는 경우
//    TranslationResult<IR0ExpResult> HandleLoc(IntermediateExp imExp)
//    {
//        var reExpResult = IntermediateExpResolvedExpTranslator.Translate(imExp, context, nodeForCallableErrorReport);
//        if (!reExpResult.IsValid(out var reExp))
//            return Error();
//
//        var result = ResolvedExpIR0LocTranslator.Translate(
//            reExp,
//            context,
//            bWrapExpAsLoc: true,
//            nodeForCallableErrorReport,
//            A0902_CallExp_CallableExpressionIsNotCallable);
//
//        if (!result.IsValid(out var locResult))
//            return Error();
//
//        var(callableLoc, callableType) = locResult;
//
//        // TODO: Lambda말고 func<>도 있다
//        var lambdaType = callableType as LambdaType;
//        if (lambdaType == null)
//        {
//            return FatalCallable(A0902_CallExp_CallableExpressionIsNotCallable);
//        }
//
//        // 일단 lambda파라미터는 params를 지원하지 않는 것으로
//        // args는 params를 지원 할 수 있음
//
//        var outer = lambdaType.GetOuter();
//        var outerTypeEnv = outer.GetTypeEnv();
//
//        // TODO: 메모리를 덜 먹는 방법으로
//        var parameters = ImmutableArray.CreateRange(lambdaType.GetParameterCount(), lambdaType.GetParameter);
//
//        var match = FuncMatcher.Match(context, outerTypeEnv, parameters, bVariadic: false, partialTypeArgs : default, argSyntaxes, new NullTypeResolver(typeArgs : default));
//
//        if (match != null)
//        {
//            return Valid(new IR0ExpResult(new R.CallLambdaExp(lambdaType.GetSymbol(), callableLoc, match.Value.Args), lambdaType.GetReturn().Type));
//        }
//        else
//        {
//            return FatalCallExp(A0401_Parameter_MismatchBetweenParamCountAndArgCount);
//        }
//    }
//
//    TranslationResult<IR0ExpResult> VisitEnumElem(EnumElemSymbol enumElem)
//    {
//        if (enumElem.IsStandalone())
//            return FatalCallable(A0902_CallExp_CallableExpressionIsNotCallable);
//
//        var fieldParamTypes = enumElem.GetConstructorParamTypes();
//
//        // 일단 lambda파라미터는 params를 지원하지 않는 것으로
//        // args는 params를 지원 할 수 있음
//        // TODO: MatchFunc에 OuterTypeEnv를 넣는 것이 나은지, fieldParamTypes에 미리 적용해서 넣는 것이 나은지
//        // paramTypes으로 typeValues를 건네 줄것이면 적용해서 넣는게 나을 것 같은데, TypeResolver 동작때문에 어떻게 될지 몰라서 일단 여기서는 적용하고 TypeEnv.None을 넘겨준다
//        var matchResult = FuncMatcher.Match(context, TypeEnv.Empty, fieldParamTypes, bVariadic: false, partialTypeArgs : default, argSyntaxes, new NullTypeResolver(typeArgs : default));
//
//        if (matchResult != null)
//        {
//            return Valid(new IR0ExpResult(new R.NewEnumElemExp(enumElem, matchResult.Value.Args), new EnumElemType(enumElem)));
//        }
//        else
//        {
//            return FatalCallExp(A0401_Parameter_MismatchBetweenParamCountAndArgCount);
//        }
//    }
//
//    TranslationResult<IR0ExpResult> VisitStruct(StructSymbol structCallable)
//    {
//        // NOTICE: 생성자 검색 (AnalyzeNewExp 부분과 비슷)
//        var structDecl = structCallable.GetDecl();
//        var candidates = FuncCandidates.Make<StructConstructorDeclSymbol, StructConstructorSymbol>(
//            structCallable, structDecl.GetConstructorCount(), structDecl.GetConstructor, partialTypeArgs: default); // TODO: 일단은 constructor의 typeArgs는 없는 것으로
//
//        var matchResult = FuncsMatcher.Match(candidates, argSyntaxes, context);
//        if (matchResult == null)
//            throw new NotImplementedException(); // 매치에 실패했습니다.
//
//        var(constructor, args) = matchResult.Value;
//        return Valid(new IR0ExpResult(new R.NewStructExp(constructor, args), new StructType(structCallable)));
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitLocalDeref(IntermediateExp.LocalDeref imExp)
//    {
//        return HandleLoc(imExp);
//    }
//
//    TranslationResult<IR0ExpResult> IIntermediateExpVisitor<TranslationResult<IR0ExpResult>>.VisitBoxDeref(IntermediateExp.BoxDeref imExp)
//    {
//        return HandleLoc(imExp);
//    }
//}

} // namespace Citron::SyntaxIR0Translator