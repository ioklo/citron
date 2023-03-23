﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Citron.Infra;
using Citron.Collections;
using Citron.Symbol;

using R = Citron.IR0;
using S = Citron.Syntax;
using static Citron.Analysis.SyntaxAnalysisErrorCode;
using IExpVisitor = Citron.Syntax.IExpVisitor;

namespace Citron.Analysis;

partial struct ExpVisitor : IExpVisitor
{
    IType? hintType;
    ScopeContext context;
    ExpResult? result;

    ExpVisitor(ScopeContext context, IType? hintType)
    {
        this.context = context;
        this.hintType = hintType;
    }

    // 결과
    public static R.Exp TranslateAsExp(S.Exp exp, ScopeContext context, IType? hintType)
    {
        var visitor = new ExpVisitor(context, hintType);
        exp.Accept(ref visitor);
        Debug.Assert(visitor.result != null);
        var result = visitor.result;

        if (result is ExpResult.NotFound)
            context.AddFatalError(A2007_ResolveIdentifier_NotFound, exp);
        else if (result is ExpResult.Error errorResult)
            visitor.HandleExpErrorResult(errorResult, exp);

        Debug.Assert(result is not ExpResult.NotFound);
        Debug.Assert(result is not ExpResult.Error);

        var rexp = result.MakeIR0Exp();
        if (rexp == null)
            throw new NotImplementedException(); // exp로 변환하기가 실패했습니다.

        return rexp;
    }

    public static (R.Loc Loc, IType Type)? TranslateAsLoc(S.Exp exp, ScopeContext context, IType? hintType, bool bWrapExpAsLoc)
    {
        var visitor = new ExpVisitor(context, hintType);        
        exp.Accept(ref visitor);
        Debug.Assert(visitor.result != null);

        return visitor.result.MakeIR0Loc(bWrapExpAsLoc);
    }

    // x
    void IExpVisitor.VisitIdentifier(S.IdentifierExp exp)
    {
        var typeArgs = BodyMisc.MakeTypeArgs(exp.TypeArgs, context);
        result = context.ResolveIdentifier(new Name.Normal(exp.Value), typeArgs);
    }

    // 'null'
    void IExpVisitor.VisitNullLiteral(S.NullLiteralExp exp)
    {
        if (hintType != null)
        {
            // int? i = null;
            if (hintType is NullableType nullableHintType)
            {
                result = new ExpResult.IR0Exp(new R.NewNullableExp(null, nullableHintType));
                return;
            }
        }

        context.AddFatalError(A2701_NullLiteralExp_CantInferNullableType, exp);
        throw new UnreachableCodeException();
    }

    void IExpVisitor.VisitBoolLiteral(S.BoolLiteralExp exp)
    {
        result = new ExpResult.IR0Exp(new R.BoolLiteralExp(exp.Value, context.GetBoolType()));
    }

    void IExpVisitor.VisitIntLiteral(S.IntLiteralExp exp)
    {
        result = new ExpResult.IR0Exp(new R.IntLiteralExp(exp.Value, context.GetIntType()));
    }
    
    void IExpVisitor.VisitString(S.StringExp exp)
    {
        var bFatal = false;

        var builder = ImmutableArray.CreateBuilder<R.StringExpElement>();
        foreach (var elem in exp.Elements)
        {
            try
            {
                var expElem = VisitStringExpElement(elem);
                builder.Add(expElem);
            }
            catch (AnalyzerFatalException)
            {
                bFatal = true;
            }
        }

        if (bFatal)
            throw new AnalyzerFatalException();

        result = new ExpResult.IR0Exp(new R.StringExp(builder.ToImmutable(), context.GetStringType()));
    }

    // int만 지원한다
    void VisitIntUnaryAssignExp(S.Exp operand, R.InternalUnaryAssignOperator op)
    {
        var locResult = ExpVisitor.TranslateAsLoc(operand, context, hintType: null, bWrapExpAsLoc: false);
        if (locResult != null)
        {
            var (loc, type) = locResult.Value;
            var intType = context.GetIntType();

            // int type 검사, exact match
            if (!type.Equals(context.GetIntType()))
                context.AddFatalError(A0601_UnaryAssignOp_IntTypeIsAllowedOnly, operand);

            result = new ExpResult.IR0Exp(new R.CallInternalUnaryAssignOperatorExp(op, loc, intType));
        }
        else
        {
            context.AddFatalError(A0602_UnaryAssignOp_AssignableExpressionIsAllowedOnly, operand);
            throw new UnreachableCodeException();
        }
    }

    void IExpVisitor.VisitUnaryOp(S.UnaryOpExp exp)
    {
        var operandExp = ExpVisitor.TranslateAsExp(exp.Operand, context, null);

        switch (exp.Kind)
        {
            case S.UnaryOpKind.LogicalNot:
                {
                    // exact match
                    if (!context.GetBoolType().Equals(operandExp.GetExpType()))
                        context.AddFatalError(A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly, exp.Operand);

                    result = new ExpResult.IR0Exp(new R.CallInternalUnaryOperatorExp(
                        R.InternalUnaryOperator.LogicalNot_Bool_Bool,
                        operandExp,
                        context.GetBoolType()
                    ));
                    return;
                }

            case S.UnaryOpKind.Minus:
                {
                    if (!context.GetIntType().Equals(operandExp.GetExpType()))
                        context.AddFatalError(A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly, exp.Operand);

                    result = new ExpResult.IR0Exp(new R.CallInternalUnaryOperatorExp(
                        R.InternalUnaryOperator.UnaryMinus_Int_Int,
                        operandExp, context.GetIntType()
                    ));
                    return;
                }

            case S.UnaryOpKind.PostfixInc: // e.m++ 등
                VisitIntUnaryAssignExp(exp.Operand, R.InternalUnaryAssignOperator.PostfixInc_Int_Int);
                return;

            case S.UnaryOpKind.PostfixDec:
                VisitIntUnaryAssignExp(exp.Operand, R.InternalUnaryAssignOperator.PostfixDec_Int_Int);
                return;

            case S.UnaryOpKind.PrefixInc:
                VisitIntUnaryAssignExp(exp.Operand, R.InternalUnaryAssignOperator.PrefixInc_Int_Int);
                return;

            case S.UnaryOpKind.PrefixDec:
                VisitIntUnaryAssignExp(exp.Operand, R.InternalUnaryAssignOperator.PrefixDec_Int_Int);
                return;

            default:
                throw new UnreachableCodeException();
        }
    }

    void VisitAssignBinaryOpExp(S.BinaryOpExp exp)
    {
        // syntax 에서는 exp로 보이지만, R로 변환할 경우 Location 명령이어야 한다
        var loc0  = ExpVisitor.TranslateAsLoc(exp.Operand0, context, hintType: null, bWrapExpAsLoc: true);

        if (loc0 != null)
        {
            // 안되는거 체크
            var (destLoc, destType) = loc0.Value;

            switch (destLoc)
            {
                // int x = 0; var l = () { x = 3; }, TODO: 이거 가능하도록
                case R.LambdaMemberVarLoc:
                    context.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
                    throw new UnreachableCodeException();

                case R.ThisLoc:          // this = x;
                    context.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
                    throw new UnreachableCodeException();

                case R.TempLoc:
                    context.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
                    throw new UnreachableCodeException();
            }
            
            var srcExp = ExpVisitor.TranslateAsExp(exp.Operand1, context, destType);
            var wrappedSrcExp = CastExp_Exp(srcExp, destType, exp);

            result = new ExpResult.IR0Exp(new R.AssignExp(destLoc, wrappedSrcExp));
        }
        else
        {
            context.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
            throw new UnreachableCodeException();
        }
    }

    void IExpVisitor.VisitBinaryOp(S.BinaryOpExp exp)
    {
        // 1. Assign 먼저 처리
        if (exp.Kind == S.BinaryOpKind.Assign)
        {
            VisitAssignBinaryOpExp(exp);
            return;
        }

        var operandExp0 = ExpVisitor.TranslateAsExp(exp.Operand0, context, hintType: null);
        var operandExp1 = ExpVisitor.TranslateAsExp(exp.Operand1, context, hintType: null);

        // 2. NotEqual 처리
        if (exp.Kind == S.BinaryOpKind.NotEqual)
        {
            var equalInfos = context.GetBinaryOpInfos(S.BinaryOpKind.Equal);
            foreach (var info in equalInfos)
            {
                var castExp0 = BodyMisc.TryCastExp_Exp(operandExp0, info.OperandType0);
                var castExp1 = BodyMisc.TryCastExp_Exp(operandExp1, info.OperandType1);

                // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
                if (castExp0 != null && castExp1 != null)
                {
                    var equalExp = new R.CallInternalBinaryOperatorExp(info.IR0Operator, castExp0, castExp1, context.GetBoolType());
                    result = new ExpResult.IR0Exp(new R.CallInternalUnaryOperatorExp(R.InternalUnaryOperator.LogicalNot_Bool_Bool, equalExp, info.ResultType));
                    return;
                }
            }
        }

        // 3. InternalOperator에서 검색            
        var matchedInfos = context.GetBinaryOpInfos(exp.Kind);
        foreach (var info in matchedInfos)
        {
            var castExp0 = BodyMisc.TryCastExp_Exp(operandExp0, info.OperandType0);
            var castExp1 = BodyMisc.TryCastExp_Exp(operandExp1, info.OperandType1);

            // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
            if (castExp0 != null && castExp1 != null)
            {
                result = new ExpResult.IR0Exp(new R.CallInternalBinaryOperatorExp(info.IR0Operator, castExp0, castExp1, info.ResultType));
                return;
            }
        }

        // Operator를 찾을 수 없습니다
        context.AddFatalError(A0802_BinaryOp_OperatorNotFound, exp);
        throw new UnreachableCodeException();
    }

    (TFuncSymbol Func, ImmutableArray<R.Argument> Args) InternalMatchFunc<TFuncDeclSymbol, TFuncSymbol>(
        ImmutableArray<DeclAndConstructor<TFuncDeclSymbol, TFuncSymbol>> declAndConstructors,
        ImmutableArray<IType> typeArgsForMatch,
        ImmutableArray<S.Argument> sargs, S.ISyntaxNode nodeForErrorReport)
        where TFuncDeclSymbol : IFuncDeclSymbol
    {
        // TODO: 메모리 소비 제거
        var decls = ImmutableArray.CreateRange(declAndConstructors, declAndConstructor => declAndConstructor.GetDecl());

        // outer가 없으므로 outerTypeEnv는 None이다
        var result = FuncMatcher.MatchIndex(context, TypeEnv.Empty, decls, sargs, typeArgsForMatch);

        switch (result)
        {
            case FuncMatchIndexResult.MultipleCandidates:
                context.AddFatalError(A0901_CallExp_MultipleCandidates, nodeForErrorReport);
                throw new UnreachableCodeException();

            case FuncMatchIndexResult.NotFound:
                context.AddFatalError(A0906_CallExp_NotFound, nodeForErrorReport);
                throw new UnreachableCodeException();

            case FuncMatchIndexResult.Success successResult:
                var func = declAndConstructors[successResult.Index].MakeSymbol(successResult.TypeArgs);
                return (func, successResult.Args);

            default:
                throw new UnreachableCodeException();
        }
    }

    // CallExp분석에서 Callable이 GlobalMemberFuncs인 경우 처리
    void VisitCallExpGlobalFuncsCallable(ExpResult.GlobalFuncs funcs, ImmutableArray<S.Argument> sargs, S.ISyntaxNode nodeForErrorReport)
    {
        var (func, args) = InternalMatchFunc(funcs.Infos, funcs.ParitalTypeArgs, sargs, nodeForErrorReport);

        if (!context.CanAccess(func))
            context.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);

        result = new ExpResult.IR0Exp(new R.CallGlobalFuncExp(func, args));
    }

    // CallExp분석에서 Callable이 ClassMemberFuncs인 경우 처리
    void VisitCallExpClassMemberFuncsCallable(ExpResult.ClassMemberFuncs funcs, ImmutableArray<S.Argument> sargs, S.ISyntaxNode nodeForErrorReport)
    {
        var (func, args) = InternalMatchFunc(funcs.Infos, funcs.ParitalTypeArgs, sargs, nodeForErrorReport);

        if (context.CanAccess(func))
            context.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);

        if (funcs.HasExplicitInstance) // x.F, C.F 등 인스턴스 부분이 명시적으로 정해졌다면
        {
            // static함수를 인스턴스를 통해 접근하려고 했을 경우 에러 처리
            if (func.IsStatic() && funcs.ExplicitInstance != null)
                context.AddFatalError(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance, nodeForErrorReport);

            // 인스턴스 함수를 인스턴스 없이 호출하려고 했다면
            if (!func.IsStatic() && funcs.ExplicitInstance == null)
                context.AddFatalError(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType, nodeForErrorReport);

            result = new ExpResult.IR0Exp(new R.CallClassMemberFuncExp(func, funcs.ExplicitInstance, args));
        }
        else // F 로 인스턴스를 명시적으로 정하지 않았다면 
        {
            if (func.IsStatic()) // 정적함수이면 인스턴스에 null
            {
                result = new ExpResult.IR0Exp(new R.CallClassMemberFuncExp(func, null, args));
            }
            else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
            {   
                result = new ExpResult.IR0Exp(new R.CallClassMemberFuncExp(func, new R.ThisLoc(), args));
            }
        }

        //if (func.IsSequence)
        //{
        //    // TODO: funcValue.RetType을 쓰면 의미가 와닿지 않는데, 쉽게 실수 할 수 있을 것 같다
        //    var seqTypeValue = context.GetSeqTypeValue(funcValue.MakeRPath(), funcValue.GetRetType());
        //    return new ExpResult.Exp(new R.CallSeqFuncExp(funcValue.MakeRPath(), funcsResult.Instance, matchedFunc.Args), seqTypeValue);
        //}
        //else
        //{
        //    return new ExpResult.Exp(new R.CallClassMemberFuncExp(func, funcs.Instance, args));
        //}
    }

    // CallExp분석에서 Callable이 StructMemberFuncs인 경우 처리
    void VisitCallExpStructMemberFuncsCallable(ExpResult.StructMemberFuncs funcs, ImmutableArray<S.Argument> sargs, S.ISyntaxNode nodeForErrorReport)
    {
        var (func, args) = InternalMatchFunc(funcs.Infos, funcs.ParitalTypeArgs, sargs, nodeForErrorReport);

        if (context.CanAccess(func))
            context.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);

        // static 함수를 호출하는 위치가 선언한 타입 내부라면 체크하지 않고 넘어간다 (멤버 호출이 아닌 경우)
        if (funcs.HasExplicitInstance)
        {
            // static this 체크
            if (func.IsStatic() && funcs.ExplicitInstance != null)
                context.AddFatalError(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance, nodeForErrorReport);

            // 반대의 경우도 체크
            if (!func.IsStatic() && funcs.ExplicitInstance == null)
                context.AddFatalError(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType, nodeForErrorReport);

            result = new ExpResult.IR0Exp(new R.CallStructMemberFuncExp(func, funcs.ExplicitInstance, args));
        }
        else
        {
            if (func.IsStatic()) // 정적함수이면 인스턴스에 null
            {
                result = new ExpResult.IR0Exp(new R.CallStructMemberFuncExp(func, null, args));
            }
            else // 인스턴스 함수이면 인스턴스에 this가 들어간다 B.F 로 접근할 경우 어떻게 하나
            {
                var thisLoc = new R.ThisLoc();
                result = new ExpResult.IR0Exp(new R.CallStructMemberFuncExp(func, thisLoc, args));
            }
        }

        //if (func.IsSequence)
        //{
        //    // TODO: funcValue.RetType을 쓰면 의미가 와닿지 않는데, 쉽게 실수 할 수 있을 것 같다
        //    var seqTypeValue = context.GetSeqTypeValue(funcValue.MakeRPath(), funcValue.GetRetType());
        //    return new ExpResult.Exp(new R.CallSeqFuncExp(funcValue.MakeRPath(), funcsResult.Instance, matchedFunc.Args), seqTypeValue);
        //}
        //else
        //{
        //    return new ExpResult.Exp(new R.CallStructMemberFuncExp(func, funcs.ExplicitInstance, args));
        //}
    }

    // CallExp 분석에서 Callable이 Exp인 경우 처리
    void VisitCallExpExpCallable(R.Loc callableLoc, IType callable, ImmutableArray<S.Argument> sargs, S.CallExp nodeForErrorReport)
    {
        // TODO: Lambda말고 func<>도 있다
        var lambdaType = callable as LambdaType;
        if (lambdaType == null)
        {
            context.AddFatalError(A0902_CallExp_CallableExpressionIsNotCallable, nodeForErrorReport.Callable);
            throw new UnreachableCodeException();
        }

        var lambdaSymbol = lambdaType.Symbol;

        // 일단 lambda파라미터는 params를 지원하지 않는 것으로
        // args는 params를 지원 할 수 있음

        var outer = lambdaType.Symbol.GetOuter();
        var outerTypeEnv = outer != null ? outer.GetTypeEnv() : TypeEnv.Empty;

        // TODO: 메모리를 덜 먹는 방법으로
        var parameters = ImmutableArray.CreateRange(lambdaSymbol.GetParameterCount, lambdaSymbol.GetParameter);

        var match = FuncMatcher.Match(context, outerTypeEnv, parameters, null, default, sargs);

        if (match != null)
        {
            result = new ExpResult.IR0Exp(new R.CallValueExp(lambdaSymbol, callableLoc, match.Value.Args));
        }
        else
        {
            context.AddFatalError(A0401_Parameter_MismatchBetweenParamCountAndArgCount, nodeForErrorReport);
            throw new UnreachableCodeException();
        }
    }

    void VisitCallExpEnumElemCallable(EnumElemSymbol enumElem, ImmutableArray<S.Argument> sargs, S.ISyntaxNode nodeForErrorReport)
    {
        if (enumElem.IsStandalone())
            context.AddFatalError(A0902_CallExp_CallableExpressionIsNotCallable, nodeForErrorReport);

        var fieldParamTypes = enumElem.GetConstructorParamTypes();

        // 일단 lambda파라미터는 params를 지원하지 않는 것으로
        // args는 params를 지원 할 수 있음
        // TODO: MatchFunc에 OuterTypeEnv를 넣는 것이 나은지, fieldParamTypes에 미리 적용해서 넣는 것이 나은지
        // paramTypes으로 typeValues를 건네 줄것이면 적용해서 넣는게 나을 것 같은데, TypeResolver 동작때문에 어떻게 될지 몰라서 일단 여기서는 적용하고 TypeEnv.None을 넘겨준다
        var matchResult = FuncMatcher.Match(context, TypeEnv.Empty, fieldParamTypes, null, default, sargs);

        if (matchResult != null)
        {
            result = new ExpResult.IR0Exp(new R.NewEnumElemExp(enumElem, matchResult.Value.Args));
        }
        else
        {
            context.AddError(A0401_Parameter_MismatchBetweenParamCountAndArgCount, nodeForErrorReport);
            throw new UnreachableCodeException();
        }
    }

    void VisitCallExpStructCallable(StructSymbol structCallable, ImmutableArray<S.Argument> sargs, S.CallExp nodeForErrorReport)
    {
        // NOTICE: 생성자 검색 (AnalyzeNewExp 부분과 비슷)
        var structDecl = structCallable.GetDecl();

        var constructorDecls = ImmutableArray.CreateRange(structDecl.GetConstructorCount, structDecl.GetConstructor);

        var matchResult = FuncMatcher.MatchIndex(context, structCallable.GetTypeEnv(), constructorDecls, sargs, default);

        switch (matchResult)
        {
            case FuncMatchIndexResult.MultipleCandidates:
                context.AddFatalError(A0907_CallExp_MultipleMatchedStructConstructors, nodeForErrorReport);
                break;

            case FuncMatchIndexResult.NotFound:
                context.AddFatalError(A0905_CallExp_NoMatchedStructConstructorFound, nodeForErrorReport);
                break;

            case FuncMatchIndexResult.Success successResult:
                // 지금은 constructor가 typeArgs를 받지 않아서 structCallable에서 곧바로 가져올 수 있지만,
                // TypeArgs가 추가된다면 Constructor도 Instantiate 해야 하고, StructSymbol.GetConstructor를 제거해야한다                        
                // var constructorDecl = structDecl.GetConstructor(successResult.Index)
                // SymbolInstantiator.Instantiate(factory, structCallable, constructorDecl, successResult.TypeArgs);
                var constructor = structCallable.GetConstructor(successResult.Index);

                if (!context.CanAccess(constructor))
                    context.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);

                result = new ExpResult.IR0Exp(new R.NewStructExp(constructor, successResult.Args));
                break;

            default:
                throw new UnreachableCodeException();
        }

        
    }

    void IExpVisitor.VisitCall(S.CallExp exp)
    {
        // 여기서 분석해야 할 것은 
        // 1. 해당 Exp가 함수인지, 변수인지, 함수라면 FuncId를 넣어준다
        // 2. Callable 인자에 맞게 잘 들어갔는지 -> 완료
        // 3. 잘 들어갔다면 리턴타입 -> 완료

        var callableResult = ExpVisitor.VisitExp(exp.Callable, context, hintType: null);

        // TODO: 함수 이름을 먼저 찾고, 타입 힌트에 따라서 Exp를 맞춰봐야 한다
        // 함수 이름을 먼저 찾는가
        // Argument 타입을 먼저 알아내야 하는가
        // F(First); F(E.First); 가 되게 하려면 이름으로 먼저 찾고, 인자타입을 맞춰봐야 한다

        switch (callableResult)
        {
            case ExpResult.NotFound:
                context.AddFatalError(A0906_CallExp_NotFound, exp.Callable);
                break;

            case ExpResult.Error errorResult:
                HandleExpErrorResult(errorResult, exp.Callable);
                break;

            case ExpResult.Namespace:
                context.AddFatalError(A0902_CallExp_CallableExpressionIsNotCallable, exp.Callable);
                break;

            case ExpResult.Class:
                context.AddFatalError(A0902_CallExp_CallableExpressionIsNotCallable, exp.Callable);
                break;

            // callable이 타입으로 계산되면 Struct과 EnumElem의 경우 생성자 호출을 한다
            case ExpResult.Struct structResult:
                VisitCallExpStructCallable(structResult.Symbol, exp.Args, exp);
                break;

            case ExpResult.Enum:
                context.AddFatalError(A0902_CallExp_CallableExpressionIsNotCallable, exp.Callable);
                break;

            // callable이 타입으로 계산되면 Struct과 EnumElem의 경우 생성자 호출을 한다
            case ExpResult.EnumElem enumElemResult:
                VisitCallExpEnumElemCallable(enumElemResult.Symbol, exp.Args, exp);
                break;

            case ExpResult.EnumElemMemberVar enumElemMemberVarResult:
                {
                    var locResult = enumElemMemberVarResult.MakeIR0Loc(bWrapExpAsLoc: false);
                    Debug.Assert(locResult != null);
                    VisitCallExpExpCallable(locResult.Value.Loc, locResult.Value.Type, exp.Args, exp);
                    break;
                }

            case ExpResult.GlobalFuncs globalFuncsResult:
                VisitCallExpGlobalFuncsCallable(globalFuncsResult, exp.Args, exp);
                break;

            case ExpResult.ClassMemberFuncs classMemberfuncsResult:
                VisitCallExpClassMemberFuncsCallable(classMemberfuncsResult, exp.Args, exp);
                break;

            case ExpResult.StructMemberFuncs structMemberFuncsResult:
                VisitCallExpStructMemberFuncsCallable(structMemberFuncsResult, exp.Args, exp);
                break;

            case ExpResult.IR0Exp expResult:
                var tempLoc = new R.TempLoc(expResult.Exp);
                VisitCallExpExpCallable(tempLoc, expResult.Exp.GetExpType(), exp.Args, exp);
                break;

            case ExpResult.IR0Loc locResult:
                VisitCallExpExpCallable(locResult.Loc, locResult.Type, exp.Args, exp);
                break;

            default:
                throw new UnreachableCodeException();
        }
    }

    void IExpVisitor.VisitLambda(S.LambdaExp exp)
    {
        // TODO: 리턴 타입과 인자타입은 타입 힌트를 반영해야 한다
        IType? retType = null;

        var visitor = new LambdaVisitor(retType, exp.Params, exp.Body, context, nodeForErrorReport: exp);
        var (lambdaSymbol, args) = visitor.Visit();
        result = new ExpResult.IR0Exp(new R.LambdaExp(lambdaSymbol, args));
    }

    void IExpVisitor.VisitIndexer(S.IndexerExp exp)
    {
        var objResult = TranslateAsLoc(exp.Object, context, hintType: null, bWrapExpAsLoc: true);
        if (objResult == null)
        {
            throw new NotImplementedException(); // TODO: indexer의 앞부분에는 location이 와야 합니다
        }

        var indexResult = TranslateAsExp(exp.Index, context, hintType: null);
        var castIndexResult = CastExp_Exp(indexResult, context.GetIntType(), exp.Index);

        // TODO: custom indexer를 만들수 있으면 좋은가
        // var memberResult = objResult.TypeSymbol.QueryMember(new M.Name(M.SpecialName.IndexerGet, null), 0);

        // 리스트 타입의 경우,
        if (context.IsListType(objResult.Value.Type, out var itemType))
        {
            result = new ExpResult.IR0Loc(new R.ListIndexerLoc(objResult.Value.Loc, castIndexResult), itemType);
        }
        else
        {
            throw new NotImplementedException();
        }

        //// objTypeValue에 indexTypeValue를 인자로 갖고 있는 indexer가 있는지
        //if (!context.TypeValueService.GetMemberFuncValue(objType, SpecialNames.IndexerGet, ImmutableArray<TypeValue>.Empty, out var funcValue))
        //{
        //    context.ErrorCollector.Add(exp, "객체에 indexer함수가 없습니다");
        //    return false;
        //}

        //if (IsFuncStatic(funcValue.FuncId))
        //{
        //    Debug.Fail("객체에 indexer가 있는데 Static입니다");
        //    return false;
        //}

        //var funcTypeValue = context.TypeValueService.GetTypeValue(funcValue);

        //if (!analyzer.CheckParamTypes(exp, funcTypeValue.Params, new[] { indexType }))
        //    return false;

        //var listType = analyzer.GetListTypeValue()

        //// List타입인가 확인
        //if (analyzer.IsAssignable(listType, objType))
        //{
        //    var objTypeId = context.GetTypeId(objType);
        //    var indexTypeId = context.GetTypeId(indexType);

        //    outExp = new ListIndexerExp(new ExpInfo(obj, objTypeId), new ExpInfo(index, indexTypeId));
        //    outTypeValue = funcTypeValue.Return;
        //    return true;
        //}
    }

    void HandleItemQueryResultError(SymbolQueryResult.Error error, S.ISyntaxNode nodeForErrorReport)
    {
        switch (error)
        {
            case SymbolQueryResult.Error.MultipleCandidates:
                context.AddFatalError(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, nodeForErrorReport);
                throw new UnreachableCodeException();

            case SymbolQueryResult.Error.VarWithTypeArg:
                context.AddFatalError(A2002_ResolveIdentifier_VarWithTypeArg, nodeForErrorReport);
                throw new UnreachableCodeException();

            default:
                throw new UnreachableCodeException();
        }
    }

    // exp.x
    void VisitMemberExpLocParent(S.MemberExp memberExp, R.Loc parentLoc, IType instanceType)
    {
        var typeArgs = BodyMisc.MakeTypeArgs(memberExp.MemberTypeArgs, context);
        var memberResult = instanceType.QueryMember(new Name.Normal(memberExp.MemberName), typeArgs.Length);

        switch (memberResult)
        {
            case SymbolQueryResult.Error errorResult:
                HandleItemQueryResultError(errorResult, memberExp);
                break;

            case SymbolQueryResult.NotFound:
                context.AddFatalError(A2007_ResolveIdentifier_NotFound, memberExp);
                break;

            case SymbolQueryResult.GlobalFuncs:
                throw new UnreachableCodeException(); // 나올 수가 없다

            // exp.C
            case SymbolQueryResult.Class:
                context.AddFatalError(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance, memberExp);
                break;

            // exp.F
            case SymbolQueryResult.ClassMemberFuncs classMemberFuncsResult:
                result = new ExpResult.ClassMemberFuncs(classMemberFuncsResult.Infos, typeArgs, HasExplicitInstance: true, parentLoc);
                break;

            // exp.x
            case SymbolQueryResult.ClassMemberVar classMemberVarResult:
                {
                    var symbol = classMemberVarResult.Var;

                    // static인지 검사
                    if (symbol.IsStatic())
                        context.AddFatalError(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance, memberExp);

                    // access modifier 검사                            
                    if (!context.CanAccess(symbol))
                        context.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, memberExp);

                    result = new ExpResult.IR0Loc(new R.ClassMemberLoc(parentLoc, symbol), symbol.GetDeclType());
                    break;
                }

            // exp.S
            case SymbolQueryResult.Struct:
                context.AddFatalError(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance, memberExp);
                break;

            // exp.F
            case SymbolQueryResult.StructMemberFuncs structMemberFuncsResult:
                result = new ExpResult.StructMemberFuncs(structMemberFuncsResult.Infos, typeArgs, HasExplicitInstance: true, parentLoc);
                break;

            // exp.x
            case SymbolQueryResult.StructMemberVar structMemberVarResult:
                {
                    var symbol = structMemberVarResult.Var;

                    // static인지 검사
                    if (symbol.IsStatic())
                        context.AddFatalError(A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance, memberExp);

                    // access modifier 검사                            
                    if (!context.CanAccess(symbol))
                        context.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, memberExp);

                    result = new ExpResult.IR0Loc(new R.StructMemberLoc(parentLoc, symbol), symbol.GetDeclType());
                    break;
                }

            // exp.E
            case SymbolQueryResult.Enum:
                context.AddFatalError(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance, memberExp);
                break;

            // E exp;
            // exp.First
            case SymbolQueryResult.EnumElem:
                context.AddFatalError(A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance, memberExp);
                break;

            // E.Second exp;
            // exp.f
            case SymbolQueryResult.EnumElemMemberVar enumElemMemberVarResult:
                result = new ExpResult.IR0Loc(new R.EnumElemMemberLoc(parentLoc, enumElemMemberVarResult.Symbol), enumElemMemberVarResult.Symbol.GetDeclType());
                break;

            default:
                throw new UnreachableCodeException();
        }        
    }

    // T.x
    void VisitMemberExpTypeParent(S.MemberExp nodeForErrorReport, ITypeSymbol parentType, string memberName, ImmutableArray<S.TypeExp> stypeArgs)
    {
        var member = parentType.QueryMember(new Name.Normal(memberName), stypeArgs.Length);

        switch (member)
        {
            case SymbolQueryResult.NotFound:
                context.AddFatalError(A2007_ResolveIdentifier_NotFound, nodeForErrorReport);
                throw new UnreachableCodeException();

            case SymbolQueryResult.Error errorResult:
                HandleItemQueryResultError(errorResult, nodeForErrorReport);
                throw new UnreachableCodeException();

            // T.C
            case SymbolQueryResult.Class classResult:
                {
                    var typeArgs = BodyMisc.MakeTypeArgs(stypeArgs, context);
                    var classSymbol = classResult.ClassConstructor.Invoke(typeArgs);

                    // check access
                    if (!context.CanAccess(classSymbol))
                        context.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);

                    result = new ExpResult.Class(classSymbol);
                    break;
                }

            // T.F
            case SymbolQueryResult.ClassMemberFuncs classMemberFuncsResult:
                {
                    var typeArgs = BodyMisc.MakeTypeArgs(stypeArgs, context);
                    result = new ExpResult.ClassMemberFuncs(classMemberFuncsResult.Infos, typeArgs, HasExplicitInstance: true, null);
                    break;
                }

            // T.x
            case SymbolQueryResult.ClassMemberVar classMemberVarResult:
                {
                    var symbol = classMemberVarResult.Var;

                    if (!symbol.IsStatic())
                        context.AddFatalError(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType, nodeForErrorReport);

                    if (!context.CanAccess(symbol))
                        context.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);

                    result = new ExpResult.IR0Loc(new R.ClassMemberLoc(null, symbol), symbol.GetDeclType());
                    break;
                }

            // T.S
            case SymbolQueryResult.Struct structResult:
                {
                    var typeArgs = BodyMisc.MakeTypeArgs(stypeArgs, context);
                    var structSymbol = structResult.StructConstructor.Invoke(typeArgs);

                    // check access
                    if (!context.CanAccess(structSymbol))
                        context.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);

                    result = new ExpResult.Struct(structSymbol);
                    break;
                }

            // T.F
            case SymbolQueryResult.StructMemberFuncs structMemberFuncsResult:
                {
                    var typeArgs = BodyMisc.MakeTypeArgs(stypeArgs, context);
                    result = new ExpResult.StructMemberFuncs(structMemberFuncsResult.Infos, typeArgs, HasExplicitInstance: true, ExplicitInstance: null);
                    break;
                }

            // T.x
            case SymbolQueryResult.StructMemberVar structMemberVarResult:
                {
                    var symbol = structMemberVarResult.Var;

                    if (!symbol.IsStatic())
                        context.AddFatalError(A2005_ResolveIdentifier_CantGetInstanceMemberThroughType, nodeForErrorReport);

                    if (!context.CanAccess(symbol))
                        context.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);

                    result = new ExpResult.IR0Loc(new R.StructMemberLoc(null, symbol), symbol.GetDeclType());
                    break;
                }

            // T.E
            case SymbolQueryResult.Enum enumResult:
                {
                    var typeArgs = BodyMisc.MakeTypeArgs(stypeArgs, context);
                    var enumSymbol = enumResult.EnumConstructor.Invoke(typeArgs);

                    // check access
                    if (!context.CanAccess(enumSymbol))
                        context.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, nodeForErrorReport);

                    result = new ExpResult.Enum(enumSymbol);
                    break;
                }

            // E.First
            case SymbolQueryResult.EnumElem enumElemResult:
                result = new ExpResult.EnumElem(enumElemResult.Symbol);
                break;

            default:
                throw new UnreachableCodeException();
        }

        Debug.Assert(result != null);
    }

    static Dictionary<ExpResult, SyntaxAnalysisErrorCode> errorMap = new Dictionary<ExpResult, SyntaxAnalysisErrorCode>
        {
            { ExpResults.MultipleCandiates, A2001_ResolveIdentifier_MultipleCandidatesForIdentifier },
            { ExpResults.VarWithTypeArg, A2002_ResolveIdentifier_VarWithTypeArg },
            { ExpResults.CantGetStaticMemberThroughInstance, A2003_ResolveIdentifier_CantGetStaticMemberThroughInstance },
            { ExpResults.CantGetTypeMemberThroughInstance, A2004_ResolveIdentifier_CantGetTypeMemberThroughInstance },
            { ExpResults.CantGetInstanceMemberThroughType, A2005_ResolveIdentifier_CantGetInstanceMemberThroughType },
            { ExpResults.FuncCantHaveMember, A2006_ResolveIdentifier_FuncCantHaveMember },
            { ExpResults.CantGetThis, A2010_ResolveIdentifier_ThisIsNotInTheContext }
        };

    void HandleExpErrorResult(ExpResult.Error errorResult, S.ISyntaxNode nodeForErrorReport)
    {
        var code = errorMap[errorResult];
        context.AddFatalError(code, nodeForErrorReport);
    }

    // exp를 돌려주는 버전
    // parent."x"<>
    void IExpVisitor.VisitMember(S.MemberExp memberExp)
    {
        var parentResult = ExpVisitor.VisitExp(memberExp.Parent, context, hintType: null);

        // Loc으로 변환하기 전에 미리 처리할 경우들을 미리 처리한다
        switch (parentResult)
        {
            // VisitMemberTypeParent를 호출하는 경우,
            case ExpResult.Class classResult:
                VisitMemberExpTypeParent(memberExp, classResult.Symbol, memberExp.MemberName, memberExp.MemberTypeArgs);
                break;

            case ExpResult.Struct structResult:
                VisitMemberExpTypeParent(memberExp, structResult.Symbol, memberExp.MemberName, memberExp.MemberTypeArgs);
                break;

            case ExpResult.Enum enumResult:
                VisitMemberExpTypeParent(memberExp, enumResult.Symbol, memberExp.MemberName, memberExp.MemberTypeArgs);
                break;

            // 에러
            case ExpResult.NotFound:
                context.AddFatalError(A2007_ResolveIdentifier_NotFound, memberExp.Parent);
                throw new UnreachableCodeException();

            case ExpResult.Error errorResult:
                HandleExpErrorResult(errorResult, memberExp.Parent);
                throw new UnreachableCodeException();
            
            // "ns".id
            case ExpResult.Namespace:
                throw new NotImplementedException(); // NamespaceParent를 호출해야 한다
            
            // "T".id
            case ExpResult.TypeVar:
                context.AddFatalError(A2012_ResolveIdentifier_TypeVarCantHaveMember, memberExp);
                throw new UnreachableCodeException();

            // Funcs류
            case ExpResult.GlobalFuncs:
            case ExpResult.ClassMemberFuncs:
            case ExpResult.StructMemberFuncs:            
                context.AddFatalError(A2006_ResolveIdentifier_FuncCantHaveMember, memberExp);
                throw new UnreachableCodeException();

            // 'Second.x'
            case ExpResult.EnumElem:
                context.AddFatalError(A2009_ResolveIdentifier_EnumElemCantHaveMember, memberExp);
                throw new UnreachableCodeException();

            // location으로 변환해야 할 것들
            case ExpResult.ThisVar: // "this".id
            case ExpResult.LocalVar:// "l".id
            case ExpResult.LambdaMemberVar:// "x".id 
            case ExpResult.ClassMemberVar:
            case ExpResult.StructMemberVar:
            case ExpResult.EnumElemMemberVar:
            case ExpResult.IR0Exp:
            case ExpResult.IR0Loc:
                {
                    var parentLocResult = parentResult.MakeIR0Loc(bWrapExpAsLoc: true);
                    // loc으로 변환 가능했다면
                    if (parentLocResult == null)
                        throw new UnreachableCodeException();
                    {
                        var (parentLoc, parentType) = parentLocResult.Value;
                        VisitMemberExpLocParent(memberExp, parentLoc, parentType);
                    }

                    break;
                }

            default:
                throw new UnreachableCodeException();
        }

        Debug.Assert(result != null);
    }

    void IExpVisitor.VisitList(S.ListExp exp)
    {
        var builder = ImmutableArray.CreateBuilder<R.Exp>(exp.Elems.Length);

        // TODO: 타입 힌트도 이용해야 할 것 같다
        IType? curElemType = (exp.ElemType != null) ? context.MakeType(exp.ElemType) : null;

        foreach (var elem in exp.Elems)
        {
            var elemExp = ExpVisitor.TranslateAsExp(elem, context, hintType: null);
            var elemExpType = elemExp.GetExpType();
            builder.Add(elemExp);

            if (curElemType == null)
            {
                curElemType = elemExpType;
                continue;
            }

            if (!curElemType.Equals(elemExpType))
            {
                // TODO: 둘의 공통 조상을 찾아야 하는지 결정을 못했다..
                context.AddFatalError(A1702_ListExp_MismatchBetweenElementTypes, elem);
            }
        }

        if (curElemType == null)        
            context.AddFatalError(A1701_ListExp_CantInferElementTypeWithEmptyElement, exp);

        result = new ExpResult.IR0Exp(
            new R.ListExp(builder.MoveToImmutable(), context.GetListType(curElemType))
        );
    }

    // 'new C(...)'
    void IExpVisitor.VisitNew(S.NewExp exp)
    {
        var classSymbol = context.MakeType(exp.Type) as ClassSymbol;
        if (classSymbol == null)
            context.AddFatalError(A2601_NewExp_TypeIsNotClass, exp.Type);

        // NOTICE: 생성자 검색 (AnalyzeCallExpTypeCallable 부분과 비슷)                
        var classDecl = classSymbol.GetDecl();
        var constructorDecls = ImmutableArray.CreateRange(classDecl.GetConstructorCount, classDecl.GetConstructor);

        var funcMatchResult = FuncMatcher.MatchIndex(context, classSymbol.GetTypeEnv(), constructorDecls, exp.Args, default);

        switch (funcMatchResult)
        {
            case FuncMatchIndexResult.MultipleCandidates:
                context.AddFatalError(A2603_NewExp_MultipleMatchedClassConstructors, exp);
                break;

            case FuncMatchIndexResult.NotFound:
                context.AddFatalError(A2602_NewExp_NoMatchedClassConstructor, exp);
                break;

            case FuncMatchIndexResult.Success successResult:

                var constructor = classSymbol.GetConstructor(successResult.Index);

                if (!context.CanAccess(constructor))
                    context.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, exp);

                result = new ExpResult.IR0Exp(new R.NewClassExp(constructor, successResult.Args));
                break;

            default:
                throw new UnreachableCodeException();
        }

        Debug.Assert(result != null);
    }

    public static ExpResult VisitExp(S.Exp exp, ScopeContext context, IType? hintType)
    {
        var visitor = new ExpVisitor(context, hintType);
        exp.Accept(ref visitor);

        Debug.Assert(visitor.result != null);
        return visitor.result;
    }
    
    R.StringExpElement VisitStringExpElement(S.StringExpElement elem)
    {
        var stringType = context.GetStringType();

        if (elem is S.ExpStringExpElement expElem)
        {
            var exp = ExpVisitor.TranslateAsExp(expElem.Exp, context, null);
            var expType = exp.GetExpType();

            // 캐스팅이 필요하다면 
            if (expType.Equals(context.GetIntType()))
            {
                return new R.ExpStringExpElement(
                    new R.CallInternalUnaryOperatorExp(
                        R.InternalUnaryOperator.ToString_Int_String,
                        exp,
                        stringType
                    )
                );
            }
            else if (expType.Equals(context.GetBoolType()))
            {
                return new R.ExpStringExpElement(
                        new R.CallInternalUnaryOperatorExp(
                        R.InternalUnaryOperator.ToString_Bool_String,
                        exp,
                        stringType
                    )
                );
            }
            else if (expType.Equals(context.GetStringType()))
            {
                return new R.ExpStringExpElement(exp);
            }
            else
            {
                // TODO: ToString
                context.AddFatalError(A1901_StringExp_ExpElementShouldBeBoolOrIntOrString, expElem.Exp);
            }
        }
        else if (elem is S.TextStringExpElement textElem)
        {
            return new R.TextStringExpElement(textElem.Text);
        }

        throw new UnreachableCodeException();
    }

    // 값의 겉보기 타입을 변경한다
    internal R.Exp CastExp_Exp(R.Exp exp, IType expectedType, S.ISyntaxNode nodeForErrorReport) // throws AnalyzeFatalException
    {
        var result = BodyMisc.TryCastExp_Exp(exp, expectedType);
        if (result != null) return result;

        context.AddFatalError(A2201_Cast_Failed, nodeForErrorReport);
        throw new UnreachableCodeException();
    }

    public static R.Exp? TranslateAsCastExp(S.Exp exp, ScopeContext context, IType hintType, IType targetType)
    {
        var visitor = new ExpVisitor(context, hintType);
        exp.Accept(ref visitor);
        Debug.Assert(visitor.result != null);
        var result = visitor.result;

        var rexp = result.MakeIR0Exp();
        if (rexp == null)
            throw new NotImplementedException(); // exp로 변환하기가 실패했습니다.

        return visitor.CastExp_Exp(rexp, targetType, nodeForErrorReport: exp);
    }

    // &i를 뭐로 번역할 것인가
    // &c.x // box ref
    // &s.x // local ref
    // &e.x <- 금지, 런타임에 레이아웃이 바뀔 수 있다 (추후 ref-able enum을 쓰면(레이아웃이 겹치지 않는) 되도록 허용할 수 있다)
    void IExpVisitor.VisitRef(S.RefExp exp)
    {
        // &a.b.c.d.e, 일단 innerExp를 memberLoc으로 변경하고, 다시 순회한다
        var innerResult = ExpVisitor.TranslateAsLoc(exp.InnerExp, context, hintType: null, bWrapExpAsLoc: false);
        if (innerResult == null) 
            throw new NotImplementedException(); // 에러 처리

        var (innerLoc, innerType) = innerResult.Value;

        var refExpBuilder = new BoxRefExpBuilder();
        innerLoc.Accept(ref refExpBuilder);
        refExpBuilder.exp

    }

    void IExpVisitor.VisitBox(S.BoxExp exp)
    {
        throw new NotImplementedException();
    }
}