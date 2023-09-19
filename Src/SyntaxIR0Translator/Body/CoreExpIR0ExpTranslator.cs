﻿using System;
using System.Diagnostics;

using Citron.Symbol;
using Citron.Collections;
using Pretune;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis;

// Syntax Exp -> IR0 Exp로 바꿔주는 기본적인 코드
// Deref를 적용하지 않는다. 따로 해주어야 한다
struct CoreExpIR0ExpTranslator
{
    IType? hintType;
    ScopeContext context;

    public CoreExpIR0ExpTranslator(IType? hintType, ScopeContext context)
    {
        this.hintType = hintType;
        this.context = context;
    }

    TranslationResult<IR0ExpResult> Valid(IR0ExpResult expResult)
    {
        return TranslationResult.Valid(expResult);
    }

    TranslationResult<IR0ExpResult> Error()
    {
        return TranslationResult.Error<IR0ExpResult>();
    }
    
    public TranslationResult<IR0ExpResult> TranslateNullLiteral(S.NullLiteralExp exp)
    {
        if (hintType != null)
        {
            // int? i = null;
            if (hintType is NullableType nullableHintType)
            {
                return Valid(new IR0ExpResult(new R.NullableNullLiteralExp(nullableHintType), nullableHintType));
            }
        }

        context.AddFatalError(A2701_NullLiteralExp_CantInferNullableType, exp);
        return TranslationResult.Error<IR0ExpResult>();
    }

    public IR0ExpResult TranslateBoolLiteral(S.BoolLiteralExp exp)
    {
        return new IR0ExpResult(new R.BoolLiteralExp(exp.Value), context.GetBoolType());
    }

    public IR0ExpResult TranslateIntLiteral(S.IntLiteralExp exp)
    {
        return new IR0ExpResult(new R.IntLiteralExp(exp.Value), context.GetIntType());
    }

    TranslationResult<R.StringExpElement> VisitStringExpElement(S.StringExpElement elem)
    {
        TranslationResult<R.StringExpElement> Valid(R.StringExpElement elem) => TranslationResult.Valid(elem);
        TranslationResult<R.StringExpElement> Error() => TranslationResult.Error<R.StringExpElement>();

        var stringType = context.GetStringType();

        if (elem is S.ExpStringExpElement expElem)
        {
            var reExpResult = ExpResolvedExpTranslator.Translate(expElem.Exp, context, hintType: null);
            if (!reExpResult.IsValid(out var reExp))
                return Error();

            var reExpType = reExp.GetExpType();

            // 캐스팅이 필요하다면 
            if (BodyMisc.TypeEquals(reExpType, context.GetIntType()))
            {
                var result = ResolvedExpIR0ExpTranslator.Translate(reExp, context, elem);
                if (!result.IsValid(out var expResult))
                    return Error();

                return Valid(new R.ExpStringExpElement(new R.TempLoc(
                    new R.CallInternalUnaryOperatorExp(R.InternalUnaryOperator.ToString_Int_String, expResult.Exp),
                    stringType
                )));
            }
            else if (BodyMisc.TypeEquals(reExpType, context.GetBoolType()))
            {
                var result = ResolvedExpIR0ExpTranslator.Translate(reExp, context, elem);
                if (!result.IsValid(out var expResult))
                    return Error();

                return Valid(new R.ExpStringExpElement(new R.TempLoc(
                    new R.CallInternalUnaryOperatorExp(R.InternalUnaryOperator.ToString_Bool_String, expResult.Exp),
                    stringType
                )));
            }
            else if (BodyMisc.TypeEquals(reExpType, context.GetStringType()))
            {
                var result = ResolvedExpIR0LocTranslator.Translate(reExp, context, bWrapExpAsLoc: true, elem, A2015_ResolveIdentifier_ExpressionIsNotLocation);
                if (!result.IsValid(out var locResult))
                    return Error();

                return Valid(new R.ExpStringExpElement(locResult.Loc));
            }
            else
            {
                // TODO: ToString
                context.AddFatalError(A1901_StringExp_ExpElementShouldBeBoolOrIntOrString, expElem.Exp);
                return TranslationResult.Error<R.StringExpElement>();
            }
        }
        else if (elem is S.TextStringExpElement textElem)
        {
            return TranslationResult.Valid<R.StringExpElement>(new R.TextStringExpElement(textElem.Text));
        }

        throw new UnreachableException();
    }

    public TranslationResult<IR0ExpResult> TranslateString_Exp(S.StringExp exp)
    {
        var result = TranslateString_StringExp(exp);
        if (!result.IsValid(out var rexp))
            return Error();

        return Valid(new IR0ExpResult(rexp, context.GetStringType()));
    }

    public TranslationResult<R.StringExp> TranslateString_StringExp(S.StringExp exp)
    {
        var bFatal = false;

        var builder = ImmutableArray.CreateBuilder<R.StringExpElement>();
        foreach (var elem in exp.Elements)
        {
            var expElemResult = VisitStringExpElement(elem);
            if (expElemResult.Kind == TranslationResultKind.Error)
            {
                bFatal = true;
                continue;
            }

            var expElem = expElemResult.Result!;
            builder.Add(expElem);
        }

        if (bFatal)
            return TranslationResult.Error<R.StringExp>();

        return TranslationResult.Valid(new R.StringExp(builder.ToImmutable()));
    }

    // int만 지원한다
    public TranslationResult<IR0ExpResult> VisitIntUnaryAssignExp(S.Exp operand, R.InternalUnaryAssignOperator op)
    {
        // exp를 loc으로 변환하는 일을 하면 안되지만, ref는 풀어야 한다
        // F()++; (x)
        // var& x = i; x++; (o)
        // throws NotLocationException
        var operandResult = ExpIR0LocTranslator.Translate(operand, context, hintType: null, bWrapExpAsLoc: false, A0602_UnaryAssignOp_AssignableExpressionIsAllowedOnly);
        if (!operandResult.IsValid(out var operandLocResult))
            return Error();

        var expType = operandLocResult.LocType;
        var intType = context.GetIntType();

        // int type 검사, exact match
        if (!BodyMisc.TypeEquals(expType, context.GetIntType()))
        {
            context.AddFatalError(A0601_UnaryAssignOp_IntTypeIsAllowedOnly, operand);
            return Error();
        }

        return Valid(new IR0ExpResult(new R.CallInternalUnaryAssignOperatorExp(op, operandLocResult.Loc), intType));
    }

    public TranslationResult<IR0ExpResult> TranslateUnaryOpExceptDeref(S.UnaryOpExp exp)
    {
        Debug.Assert(exp.Kind != S.UnaryOpKind.Deref);

        // ref 처리
        if (exp.Kind == S.UnaryOpKind.Ref)
        {
            var refExpResult = RefExpIR0ExpTranslator.Translate(exp.Operand, context);
            if (!refExpResult.IsValid(out var refExp))
                return Error();

            return Valid(refExp);
        }

        var operandResult = ExpIR0ExpTranslator.Translate(exp.Operand, context, hintType: null);
        if (!operandResult.IsValid(out var operandExpResult))
            return Error();

        switch (exp.Kind)
        {
            case S.UnaryOpKind.LogicalNot:
                {
                    // exact match
                    if (!BodyMisc.TypeEquals(context.GetBoolType(), operandExpResult.ExpType))
                    {
                        context.AddFatalError(A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly, exp.Operand);
                        return Error();
                    }

                    return Valid(new IR0ExpResult(
                        new R.CallInternalUnaryOperatorExp(
                            R.InternalUnaryOperator.LogicalNot_Bool_Bool,
                            operandExpResult.Exp
                        ),
                        context.GetBoolType()
                    ));
                }

            case S.UnaryOpKind.Minus:
                {
                    if (!BodyMisc.TypeEquals(context.GetIntType(), operandExpResult.ExpType))
                    {
                        context.AddFatalError(A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly, exp.Operand);
                        return Error();
                    }

                    return Valid(new IR0ExpResult(
                        new R.CallInternalUnaryOperatorExp(
                            R.InternalUnaryOperator.UnaryMinus_Int_Int,
                            operandExpResult.Exp
                        ),
                        context.GetIntType()
                    ));
                }

            case S.UnaryOpKind.PostfixInc: // e.m++ 등
                return VisitIntUnaryAssignExp(exp.Operand, R.InternalUnaryAssignOperator.PostfixInc_Int_Int);

            case S.UnaryOpKind.PostfixDec:
                return VisitIntUnaryAssignExp(exp.Operand, R.InternalUnaryAssignOperator.PostfixDec_Int_Int);

            case S.UnaryOpKind.PrefixInc:
                return VisitIntUnaryAssignExp(exp.Operand, R.InternalUnaryAssignOperator.PrefixInc_Int_Int);

            case S.UnaryOpKind.PrefixDec:
                return VisitIntUnaryAssignExp(exp.Operand, R.InternalUnaryAssignOperator.PrefixDec_Int_Int);

            default:
                throw new UnreachableException();
        }
    }

    TranslationResult<IR0ExpResult> VisitAssignBinaryOpExp(S.BinaryOpExp exp)
    {
        // syntax 에서는 exp로 보이지만, R로 변환할 경우 Location 명령이어야 한다
        var destResult = ExpIR0LocTranslator.Translate(exp.Operand0, context, hintType: null, bWrapExpAsLoc: false, A0803_BinaryOp_LeftOperandIsNotAssignable);
        if (!destResult.IsValid(out var destLocResult))
            return Error();
       
        // 안되는거 체크
        switch (destLocResult.Loc)
        {
            // int x = 0; var l = () { x = 3; }, TODO: 이거 가능하도록
            case R.LambdaMemberVarLoc:
                context.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
                return Error();

            case R.ThisLoc:          // this = x;
                context.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
                return Error();

            case R.TempLoc:
                context.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
                return Error();
        }

        var srcResult = ExpIR0ExpTranslator.Translate(exp.Operand1, context, destLocResult.LocType);
        if (!srcResult.IsValid(out var srcExpResult))
            return Error();

        var wrappedSrcExpResult = BodyMisc.CastExp_Exp(srcExpResult.Exp, srcExpResult.ExpType, destLocResult.LocType, exp, context);
        if (!wrappedSrcExpResult.IsValid(out var wrappedSrcExp))
            return Error();

        return Valid(new IR0ExpResult(new R.AssignExp(destLocResult.Loc, wrappedSrcExp), destLocResult.LocType));
    }

    public TranslationResult<IR0ExpResult> TranslateBinaryOp(S.BinaryOpExp exp)
    {
        // 1. Assign 먼저 처리
        if (exp.Kind == S.BinaryOpKind.Assign)
        {
            return VisitAssignBinaryOpExp(exp);
        }

        var operandResult0 = ExpIR0ExpTranslator.Translate(exp.Operand0, context, hintType: null);
        if (!operandResult0.IsValid(out var operandExpResult0))
            return Error();

        var operandResult1 = ExpIR0ExpTranslator.Translate(exp.Operand1, context, hintType: null);
        if (!operandResult1.IsValid(out var operandExpResult1))
            return Error();

        // 2. NotEqual 처리
        if (exp.Kind == S.BinaryOpKind.NotEqual)
        {
            var equalInfos = context.GetBinaryOpInfos(S.BinaryOpKind.Equal);
            foreach (var info in equalInfos)
            {
                var castExp0 = BodyMisc.TryCastExp_Exp(operandExpResult0.Exp, operandExpResult0.ExpType, info.OperandType0, context);
                var castExp1 = BodyMisc.TryCastExp_Exp(operandExpResult1.Exp, operandExpResult1.ExpType, info.OperandType1, context);

                // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
                if (castExp0 != null && castExp1 != null)
                {
                    var equalExp = new R.CallInternalBinaryOperatorExp(info.IR0Operator, castExp0, castExp1);

                    return Valid(new IR0ExpResult(
                        new R.CallInternalUnaryOperatorExp(
                            R.InternalUnaryOperator.LogicalNot_Bool_Bool,
                            equalExp),
                        info.ResultType
                    ));
                }
            }
        }

        // 3. InternalOperator에서 검색            
        var matchedInfos = context.GetBinaryOpInfos(exp.Kind);
        foreach (var info in matchedInfos)
        {
            var castExp0 = BodyMisc.TryCastExp_Exp(operandExpResult0.Exp, operandExpResult0.ExpType, info.OperandType0, context);
            var castExp1 = BodyMisc.TryCastExp_Exp(operandExpResult1.Exp, operandExpResult1.ExpType, info.OperandType1, context);

            // NOTICE: 우선순위별로 정렬되어 있기 때문에 먼저 매칭되는 것을 선택한다
            if (castExp0 != null && castExp1 != null)
            {
                return Valid(new IR0ExpResult(
                    new R.CallInternalBinaryOperatorExp(
                        info.IR0Operator, castExp0, castExp1
                    ),
                    info.ResultType
                ));
            }
        }

        // Operator를 찾을 수 없습니다
        context.AddFatalError(A0802_BinaryOp_OperatorNotFound, exp);
        return Error();
    }

    public TranslationResult<IR0ExpResult> TranslateLambda(S.LambdaExp expSyntax)
    {
        // TODO: 리턴 타입과 인자타입은 타입 힌트를 반영해야 한다
        IType? retType = null;

        var lambdaInfoResult = LambdaVisitor.Translate(retType, expSyntax.Params, expSyntax.Body, context, nodeForErrorReport: expSyntax);
        if (!lambdaInfoResult.IsValid(out var lambdaInfo))
            return Error();
        
        return Valid(new IR0ExpResult(new R.LambdaExp(lambdaInfo.Lambda, lambdaInfo.Args), new LambdaType(lambdaInfo.Lambda)));
    }

    public TranslationResult<IR0ExpResult> TranslateList(S.ListExp exp)
    {
        var builder = ImmutableArray.CreateBuilder<R.Exp>(exp.Elems.Length);

        // TODO: 타입 힌트도 이용해야 할 것 같다
        IType? curElemType = null;

        foreach (var elem in exp.Elems)
        {
            var elemResult = ExpIR0ExpTranslator.Translate(elem, context, hintType: null);
            if (!elemResult.IsValid(out var elemExpResult))
                return Error();

            var elemType = elemExpResult.ExpType;
                
            builder.Add(elemExpResult.Exp);

            if (curElemType == null)
            {
                curElemType = elemType;
                continue;
            }

            if (!BodyMisc.TypeEquals(curElemType, elemType))
            {
                // TODO: 둘의 공통 조상을 찾아야 하는지 결정을 못했다..
                context.AddFatalError(A1702_ListExp_MismatchBetweenElementTypes, elem);
                return Error();
            }
        }

        if (curElemType == null)
        {
            context.AddFatalError(A1701_ListExp_CantInferElementTypeWithEmptyElement, exp);
            return Error();
        }

        var listType = context.GetListType(curElemType);

        return Valid(new IR0ExpResult(new R.ListExp(builder.MoveToImmutable(), curElemType), listType));
    }

    public TranslationResult<IR0ExpResult> TranslateNew(S.NewExp exp) // throws ErrorCodeException
    {
        var classSymbol = context.MakeType(exp.Type) as ClassSymbol;
        if (classSymbol == null)
        {
            context.AddFatalError(A2601_NewExp_TypeIsNotClass, exp.Type);
            return Error();
        }
        
        var classDecl = classSymbol.GetDecl();
        
        var candidates = FuncCandidates.Make<ClassConstructorDeclSymbol, ClassConstructorSymbol>(
            classSymbol, classDecl.GetConstructorCount(), classDecl.GetConstructor, partialTypeArgs: default); // TODO: 일단은 constructor의 typeArgs는 없는 것으로

        var matchResult = FuncsMatcher.Match(candidates, exp.Args, context);
        if (matchResult == null)
            throw new NotImplementedException(); // 매치에 실패했습니다.

        var (constructor, args) = matchResult.Value;
        return Valid(new IR0ExpResult(new R.NewClassExp(constructor, args), new ClassType(classSymbol)));
    }

    public TranslationResult<IR0ExpResult> TranslateCall(S.CallExp exp)
    {
        var callableResult = ExpIntermediateExpTranslator.Translate(exp.Callable, context, hintType);
        if (!callableResult.IsValid(out var callable))
            return Error();

        return CallableAndArgsBinder.Bind(callable, exp.Args, context, exp, exp.Callable);
    }

    public TranslationResult<IR0ExpResult> TranslateBox(S.BoxExp exp)
    {
        // hintType전수
        var innerHintType = (hintType as BoxPtrType)?.GetInnerType();

        var innerResult = ExpIR0ExpTranslator.Translate(exp.InnerExp, context, innerHintType);
        if (!innerResult.IsValid(out var innerExpResult))
            return Error();

        return Valid(new IR0ExpResult(new R.BoxExp(innerExpResult.Exp, innerExpResult.ExpType), new BoxPtrType(innerExpResult.ExpType)));
    }

    public TranslationResult<IR0ExpResult> TranslateIs(S.IsExp exp)
    {
        var targetResult = ExpIR0ExpTranslator.Translate(exp.Exp, context, hintType: null);
        if (!targetResult.IsValid(out var targetExpResult))
            return Error();

        var targetExp = targetExpResult.Exp;
        var targetType = targetExpResult.ExpType;
        var testType = context.MakeType(exp.Type);

        // 5가지 케이스로 나뉜다
        if (testType is ClassType testClassType)
        {
            if (targetType is ClassType)
                return Valid(new IR0ExpResult(new R.ClassIsClassExp(targetExp, testClassType), context.GetBoolType()));
            else if (targetType is InterfaceType)
                return Valid(new IR0ExpResult(new R.InterfaceIsClassExp(targetExp, testClassType), context.GetBoolType()));
            else
                throw new NotImplementedException(); // 에러 처리
        }
        else if (testType is InterfaceType testInterfaceType)
        {
            if (targetType is ClassType)
                return Valid(new IR0ExpResult(new R.ClassIsInterfaceExp(targetExp, testInterfaceType), context.GetBoolType()));
            else if (targetType is InterfaceType)
                return Valid(new IR0ExpResult(new R.InterfaceIsInterfaceExp(targetExp, testInterfaceType), context.GetBoolType()));
            else
                throw new NotImplementedException(); // 에러 처리
        }
        else if (testType is EnumElemType testEnumElemType)
        {
            if (targetType is EnumType)
                return Valid(new IR0ExpResult(new R.EnumIsEnumElemExp(targetExp, testEnumElemType), context.GetBoolType()));
            else
                throw new NotImplementedException(); // 에러 처리
        }
        else
            throw new NotImplementedException(); // 에러 처리
    }

    public TranslationResult<IR0ExpResult> TranslateAs(S.AsExp exp)
    {
        var targetResult = ExpIR0ExpTranslator.Translate(exp.Exp, context, hintType: null);
        if (!targetResult.IsValid(out var targetExpResult))
            return Error();

        var (targetExp, targetType) = targetExpResult;
        var testType = context.MakeType(exp.Type);

        return BodyMisc.MakeAsExp(targetType, testType, targetExp);
    }
}