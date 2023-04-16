using System.Diagnostics;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;
using Citron.Infra;
using Citron.Collections;
using Citron.Symbol;

namespace Citron.Analysis;

// (IntermediateExp, Args) -> R.Exp
struct CallableAndArgsBinder : IIntermediateExpVisitor<R.Exp>
{   
    ImmutableArray<S.Argument> argSyntaxes;
    ScopeContext context;

    S.ISyntaxNode nodeForCallExpErrorReport;
    S.ISyntaxNode nodeForCallableErrorReport;

    public static R.Exp Bind(IntermediateExp callable, ImmutableArray<S.Argument> argSyntaxes, ScopeContext context, S.ISyntaxNode nodeForCallExpErrorReport, S.ISyntaxNode nodeForCallableErrorReport)
    {
        // 여기서 분석해야 할 것은 
        // 1. 해당 Exp가 함수인지, 변수인지, 함수라면 FuncId를 넣어준다
        // 2. Callable 인자에 맞게 잘 들어갔는지 -> 완료
        // 3. 잘 들어갔다면 리턴타입 -> 완료

        // TODO: 함수 이름을 먼저 찾고, 타입 힌트에 따라서 Exp를 맞춰봐야 한다
        // 함수 이름을 먼저 찾는가
        // Argument 타입을 먼저 알아내야 하는가
        // F(First); F(E.First); 가 되게 하려면 이름으로 먼저 찾고, 인자타입을 맞춰봐야 한다

        var binder = new CallableAndArgsBinder() { argSyntaxes = argSyntaxes, context = context, nodeForCallableErrorReport = nodeForCallableErrorReport, nodeForCallExpErrorReport = nodeForCallExpErrorReport };
        return callable.Accept<CallableAndArgsBinder, R.Exp>(ref binder);
    }

    R.Exp FatalCallable(SyntaxAnalysisErrorCode code)
    {
        context.AddFatalError(code, nodeForCallableErrorReport);
        throw new UnreachableException();
    }

    R.Exp FatalCallExp(SyntaxAnalysisErrorCode code)
    {
        context.AddFatalError(code, nodeForCallExpErrorReport);
        throw new UnreachableException();
    }



    R.Exp IIntermediateExpVisitor<R.Exp>.VisitClass(IntermediateExp.Class imExp)
    {
        return FatalCallable(A0902_CallExp_CallableExpressionIsNotCallable);
    }

    R.Exp IIntermediateExpVisitor<R.Exp>.VisitClassMemberFuncs(IntermediateExp.ClassMemberFuncs imExp)
    {
        return VisitClassMemberFuncs(imExp);
    }

    R.Exp IIntermediateExpVisitor<R.Exp>.VisitClassMemberVar(IntermediateExp.ClassMemberVar imExp)
    {
        return HandleLoc(imExp);
    }

    R.Exp IIntermediateExpVisitor<R.Exp>.VisitEnum(IntermediateExp.Enum imExp)
    {
        return FatalCallable(A0902_CallExp_CallableExpressionIsNotCallable);
    }

    R.Exp IIntermediateExpVisitor<R.Exp>.VisitEnumElem(IntermediateExp.EnumElem imExp)
    {
        // callable이 타입으로 계산되면 Struct과 EnumElem의 경우 생성자 호출을 한다
        return VisitCallExpEnumElemCallable(imExp.Symbol);
    }

    R.Exp IIntermediateExpVisitor<R.Exp>.VisitEnumElemMemberVar(IntermediateExp.EnumElemMemberVar imExp)
    {
        return HandleLoc(imExp);
    }
    
    R.Exp IIntermediateExpVisitor<R.Exp>.VisitGlobalFuncs(IntermediateExp.GlobalFuncs imExp)
    {
        return VisitGlobalFuncs(imExp);
    }

    R.Exp IIntermediateExpVisitor<R.Exp>.VisitIR0Exp(IntermediateExp.IR0Exp imExp)
    {
        return HandleLoc(imExp);
    }
    
    R.Exp IIntermediateExpVisitor<R.Exp>.VisitLambdaMemberVar(IntermediateExp.LambdaMemberVar imExp)
    {
        return HandleLoc(imExp);
    }

    R.Exp IIntermediateExpVisitor<R.Exp>.VisitLocalVar(IntermediateExp.LocalVar imExp)
    {
        return HandleLoc(imExp);
    }

    R.Exp IIntermediateExpVisitor<R.Exp>.VisitNamespace(IntermediateExp.Namespace imExp)
    {
        return FatalCallable(A0902_CallExp_CallableExpressionIsNotCallable);
    }
    
    R.Exp IIntermediateExpVisitor<R.Exp>.VisitStruct(IntermediateExp.Struct imExp)
    {
        // callable이 타입으로 계산되면 Struct과 EnumElem의 경우 생성자 호출을 한다
        return VisitCallExpStructCallable(imExp.Symbol);
    }

    R.Exp IIntermediateExpVisitor<R.Exp>.VisitStructMemberFuncs(IntermediateExp.StructMemberFuncs imExp)
    {
        return VisitStructMemberFuncs(imExp);
    }

    R.Exp IIntermediateExpVisitor<R.Exp>.VisitStructMemberVar(IntermediateExp.StructMemberVar imExp)
    {
        return HandleLoc(imExp);
    }

    R.Exp IIntermediateExpVisitor<R.Exp>.VisitThis(IntermediateExp.ThisVar imExp)
    {
        return FatalCallable(A0902_CallExp_CallableExpressionIsNotCallable);
    }

    R.Exp IIntermediateExpVisitor<R.Exp>.VisitTypeVar(IntermediateExp.TypeVar imExp)
    {
        return FatalCallable(A0902_CallExp_CallableExpressionIsNotCallable);
    }

    // l[0]
    R.Exp IIntermediateExpVisitor<R.Exp>.VisitListIndexer(IntermediateExp.ListIndexer imExp)
    {
        return HandleLoc(imExp);
    }

    // CallExp분석에서 Callable이 GlobalMemberFuncs인 경우 처리
    R.Exp VisitGlobalFuncs(IntermediateExp.GlobalFuncs funcs)
    {
        var (func, args) = InternalMatchFunc(funcs.Infos, funcs.ParitalTypeArgs);

        if (!context.CanAccess(func))
            return FatalCallable(A2011_ResolveIdentifier_TryAccessingPrivateMember);

        return new R.CallGlobalFuncExp(func, args);
    }

    // CallExp분석에서 Callable이 ClassMemberFuncs인 경우 처리
    R.Exp VisitClassMemberFuncs(IntermediateExp.ClassMemberFuncs funcs)
    {
        var (func, args) = InternalMatchFunc(funcs.Infos, funcs.ParitalTypeArgs);

        if (context.CanAccess(func))
            return FatalCallable(A2011_ResolveIdentifier_TryAccessingPrivateMember);

        if (funcs.HasExplicitInstance) // x.F, C.F 등 인스턴스 부분이 명시적으로 정해졌다면
        {
            // static함수를 인스턴스를 통해 접근하려고 했을 경우 에러 처리
            if (func.IsStatic() && funcs.ExplicitInstance != null)
                return FatalCallable(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance);

            // 인스턴스 함수를 인스턴스 없이 호출하려고 했다면
            if (!func.IsStatic() && funcs.ExplicitInstance == null)
                return FatalCallable(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType);

            // ResolvedExp -> RExp
            R.Loc? instance = null;

            if (funcs.ExplicitInstance != null)
            {
                instance = ResolvedExpIR0LocTranslator.Translate(funcs.ExplicitInstance, context, bWrapExpAsLoc: true, nodeForCallableErrorReport);
            }

            return new R.CallClassMemberFuncExp(func, instance, args);
        }
        else // F 로 인스턴스를 명시적으로 정하지 않았다면 
        {
            if (func.IsStatic()) // 정적함수이면 인스턴스에 null
            {
                return new R.CallClassMemberFuncExp(func, null, args);
            }
            else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
            {
                return new R.CallClassMemberFuncExp(func, new R.ThisLoc(), args);
            }
        }

        //if (func.IsSequence)
        //{
        //    // TODO: funcValue.RetType을 쓰면 의미가 와닿지 않는데, 쉽게 실수 할 수 있을 것 같다
        //    var seqTypeValue = context.GetSeqTypeValue(funcValue.MakeRPath(), funcValue.GetRetType());
        //    return new IntermediateExp.Exp(new R.CallSeqFuncExp(funcValue.MakeRPath(), funcsResult.Instance, matchedFunc.Args), seqTypeValue);
        //}
        //else
        //{
        //    return new IntermediateExp.Exp(new R.CallClassMemberFuncExp(func, funcs.Instance, args));
        //}
    }

    // CallExp분석에서 Callable이 StructMemberFuncs인 경우 처리
    R.Exp VisitStructMemberFuncs(IntermediateExp.StructMemberFuncs funcs)
    {
        var (func, args) = InternalMatchFunc(funcs.Infos, funcs.ParitalTypeArgs);

        if (context.CanAccess(func))
            return FatalCallable(A2011_ResolveIdentifier_TryAccessingPrivateMember);

        // static 함수를 호출하는 위치가 선언한 타입 내부라면 체크하지 않고 넘어간다 (멤버 호출이 아닌 경우)
        if (funcs.HasExplicitInstance)
        {
            // static this 체크
            if (func.IsStatic() && funcs.ExplicitInstance != null)
                return FatalCallable(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance);

            // 반대의 경우도 체크
            if (!func.IsStatic() && funcs.ExplicitInstance == null)
                return FatalCallable(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType);

            R.Loc? instance = null;
            if (funcs.ExplicitInstance != null)
            {
                instance = ResolvedExpIR0LocTranslator.Translate(funcs.ExplicitInstance, context, bWrapExpAsLoc: true, nodeForCallableErrorReport);
            }

            return new R.CallStructMemberFuncExp(func, instance, args);
        }
        else
        {
            if (func.IsStatic()) // 정적함수이면 인스턴스에 null
            {
                return new R.CallStructMemberFuncExp(func, null, args);
            }
            else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
            {
                var thisLoc = new R.ThisLoc();
                return new R.CallStructMemberFuncExp(func, thisLoc, args);
            }
        }

        //if (func.IsSequence)
        //{
        //    // TODO: funcValue.RetType을 쓰면 의미가 와닿지 않는데, 쉽게 실수 할 수 있을 것 같다
        //    var seqTypeValue = context.GetSeqTypeValue(funcValue.MakeRPath(), funcValue.GetRetType());
        //    return new IntermediateExp.Exp(new R.CallSeqFuncExp(funcValue.MakeRPath(), funcsResult.Instance, matchedFunc.Args), seqTypeValue);
        //}
        //else
        //{
        //    return new IntermediateExp.Exp(new R.CallStructMemberFuncExp(func, funcs.ExplicitInstance, args));
        //}
    }

    // CallExp 분석에서 Callable이 Lambda, func<>로 계산되는 경우
    R.Exp HandleLoc(IntermediateExp imExp)
    {   
        var reExp = IntermediateExpResolvedExpTranslator.Translate(imExp, context, nodeForCallableErrorReport);
        var loc = ResolvedExpIR0LocTranslator.Translate(reExp, context, bWrapExpAsLoc: true, nodeForCallableErrorReport);

        var callableLoc = loc;
        var callable = reExp.GetExpType();

        // TODO: Lambda말고 func<>도 있다
        var lambdaType = callable as LambdaType;
        if (lambdaType == null)
        {
            return FatalCallable(A0902_CallExp_CallableExpressionIsNotCallable);
        }

        var lambdaSymbol = lambdaType.Symbol;

        // 일단 lambda파라미터는 params를 지원하지 않는 것으로
        // args는 params를 지원 할 수 있음

        var outer = lambdaType.Symbol.GetOuter();
        var outerTypeEnv = outer != null ? outer.GetTypeEnv() : TypeEnv.Empty;

        // TODO: 메모리를 덜 먹는 방법으로
        var parameters = ImmutableArray.CreateRange(lambdaSymbol.GetParameterCount, lambdaSymbol.GetParameter);

        var match = FuncMatcher.Match(context, outerTypeEnv, parameters, variadicParamIndex: null, typeArgs: default, argSyntaxes);

        if (match != null)
        {
            return new R.CallValueExp(lambdaSymbol, callableLoc, match.Value.Args);
        }
        else
        {
            return FatalCallExp(A0401_Parameter_MismatchBetweenParamCountAndArgCount);
        }
    }
    
    R.Exp VisitCallExpEnumElemCallable(EnumElemSymbol enumElem)
    {
        if (enumElem.IsStandalone())
            return FatalCallable(A0902_CallExp_CallableExpressionIsNotCallable);

        var fieldParamTypes = enumElem.GetConstructorParamTypes();

        // 일단 lambda파라미터는 params를 지원하지 않는 것으로
        // args는 params를 지원 할 수 있음
        // TODO: MatchFunc에 OuterTypeEnv를 넣는 것이 나은지, fieldParamTypes에 미리 적용해서 넣는 것이 나은지
        // paramTypes으로 typeValues를 건네 줄것이면 적용해서 넣는게 나을 것 같은데, TypeResolver 동작때문에 어떻게 될지 몰라서 일단 여기서는 적용하고 TypeEnv.None을 넘겨준다
        var matchResult = FuncMatcher.Match(context, TypeEnv.Empty, fieldParamTypes, null, default, argSyntaxes);

        if (matchResult != null)
        {
            return new R.NewEnumElemExp(enumElem, matchResult.Value.Args);
        }
        else
        {
            context.AddError(A0401_Parameter_MismatchBetweenParamCountAndArgCount, nodeForCallExpErrorReport);
            throw new UnreachableException();
        }
    }

    R.Exp VisitCallExpStructCallable(StructSymbol structCallable)
    {
        // NOTICE: 생성자 검색 (AnalyzeNewExp 부분과 비슷)
        var structDecl = structCallable.GetDecl();

        var constructorDecls = ImmutableArray.CreateRange(structDecl.GetConstructorCount, structDecl.GetConstructor);

        var matchResult = FuncMatcher.MatchIndex(context, structCallable.GetTypeEnv(), constructorDecls, argSyntaxes, default);

        switch (matchResult)
        {
            case FuncMatchIndexResult.MultipleCandidates:
                return FatalCallable(A0907_CallExp_MultipleMatchedStructConstructors);

            case FuncMatchIndexResult.NotFound:
                return FatalCallable(A0905_CallExp_NoMatchedStructConstructorFound);

            case FuncMatchIndexResult.Success successResult:
                // 지금은 constructor가 typeArgs를 받지 않아서 structCallable에서 곧바로 가져올 수 있지만,
                // TypeArgs가 추가된다면 Constructor도 Instantiate 해야 하고, StructSymbol.GetConstructor를 제거해야한다                        
                // var constructorDecl = structDecl.GetConstructor(successResult.Index)
                // SymbolInstantiator.Instantiate(factory, structCallable, constructorDecl, successResult.TypeArgs);
                var constructor = structCallable.GetConstructor(successResult.Index);

                if (!context.CanAccess(constructor))
                    return FatalCallable(A2011_ResolveIdentifier_TryAccessingPrivateMember);

                return new R.NewStructExp(constructor, successResult.Args);
        }

        throw new UnreachableException();
    }

    (TFuncSymbol Func, ImmutableArray<R.Argument> Args) InternalMatchFunc<TFuncDeclSymbol, TFuncSymbol>(
        ImmutableArray<DeclAndConstructor<TFuncDeclSymbol, TFuncSymbol>> declAndConstructors,
        ImmutableArray<IType> typeArgsForMatch)
        where TFuncDeclSymbol : IFuncDeclSymbol
    {
        // TODO: 메모리 소비 제거
        var decls = ImmutableArray.CreateRange(declAndConstructors, declAndConstructor => declAndConstructor.GetDecl());

        // outer가 없으므로 outerTypeEnv는 None이다
        var result = FuncMatcher.MatchIndex(context, TypeEnv.Empty, decls, argSyntaxes, typeArgsForMatch);

        switch (result)
        {
            case FuncMatchIndexResult.MultipleCandidates:
                context.AddFatalError(A0901_CallExp_MultipleCandidates, nodeForCallableErrorReport);
                throw new UnreachableException();

            case FuncMatchIndexResult.NotFound:
                context.AddFatalError(A0906_CallExp_NotFound, nodeForCallableErrorReport);
                throw new UnreachableException();

            case FuncMatchIndexResult.Success successResult:
                var func = declAndConstructors[successResult.Index].MakeSymbol(successResult.TypeArgs);
                return (func, successResult.Args);

            default:
                throw new UnreachableException();
        }
    }

    
}