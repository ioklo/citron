using System;
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

    TranslationResult<R.Exp> Valid(R.Exp exp)
    {
        return TranslationResult.Valid(exp);
    }

    TranslationResult<R.Exp> Error()
    {
        return TranslationResult.Error<R.Exp>();
    }
    
    public TranslationResult<R.Exp> TranslateNullLiteral(S.NullLiteralExp exp)
    {
        if (hintType != null)
        {
            // int? i = null;
            if (hintType is NullableType nullableHintType)
            {
                return Valid(new R.NewNullableExp(null, nullableHintType));
            }
        }

        context.AddFatalError(A2701_NullLiteralExp_CantInferNullableType, exp);
        return TranslationResult.Error<R.Exp>();
    }

    public R.Exp TranslateBoolLiteral(S.BoolLiteralExp exp)
    {
        return new R.BoolLiteralExp(exp.Value, context.GetBoolType());
    }

    public R.Exp TranslateIntLiteral(S.IntLiteralExp exp)
    {
        return new R.IntLiteralExp(exp.Value, context.GetIntType());
    }

    TranslationResult<R.StringExpElement> VisitStringExpElement(S.StringExpElement elem)
    {
        var stringType = context.GetStringType();

        if (elem is S.ExpStringExpElement expElem)
        {
            var result = ExpIR0ExpTranslator.Translate(expElem.Exp, context, hintType: null);
            if (!result.IsValid(out var exp))
                return TranslationResult.Error<R.StringExpElement>();
            
            var expType = exp.GetExpType();

            // 캐스팅이 필요하다면 
            if (expType.Equals(context.GetIntType()))
            {
                return TranslationResult.Valid<R.StringExpElement>(new R.ExpStringExpElement(
                    new R.CallInternalUnaryOperatorExp(
                        R.InternalUnaryOperator.ToString_Int_String,
                        exp,
                        stringType
                    )
                ));
            }
            else if (expType.Equals(context.GetBoolType()))
            {
                return TranslationResult.Valid<R.StringExpElement>(new R.ExpStringExpElement(
                        new R.CallInternalUnaryOperatorExp(
                        R.InternalUnaryOperator.ToString_Bool_String,
                        exp,
                        stringType
                    )
                ));
            }
            else if (expType.Equals(context.GetStringType()))
            {
                return TranslationResult.Valid<R.StringExpElement>(new R.ExpStringExpElement(exp));
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

    public TranslationResult<R.Exp> TranslateString_Exp(S.StringExp exp)
    {
        var result = TranslateString_StringExp(exp);
        if (!result.IsValid(out var rexp))
            return Error();

        return Valid(rexp);
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

        return TranslationResult.Valid(new R.StringExp(builder.ToImmutable(), context.GetStringType()));
    }

    // int만 지원한다
    public TranslationResult<R.Exp> VisitIntUnaryAssignExp(S.Exp operand, R.InternalUnaryAssignOperator op)
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
        if (!expType.Equals(context.GetIntType()))
        {
            context.AddFatalError(A0601_UnaryAssignOp_IntTypeIsAllowedOnly, operand);
            return Error();
        }

        return Valid(new R.CallInternalUnaryAssignOperatorExp(op, operandLocResult.Loc, intType));
    }

    public TranslationResult<R.Exp> TranslateUnaryOpExceptDeref(S.UnaryOpExp exp)
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

        var operandExpResult = ExpIR0ExpTranslator.Translate(exp.Operand, context, hintType: null);
        if (!operandExpResult.IsValid(out var operandExp))
            return Error();

        switch (exp.Kind)
        {
            case S.UnaryOpKind.LogicalNot:
                {
                    // exact match
                    if (!context.GetBoolType().Equals(operandExp.GetExpType()))
                    {
                        context.AddFatalError(A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly, exp.Operand);
                        return Error();
                    }

                    return Valid(new R.CallInternalUnaryOperatorExp(
                        R.InternalUnaryOperator.LogicalNot_Bool_Bool,
                        operandExp,
                        context.GetBoolType()
                    ));
                }

            case S.UnaryOpKind.Minus:
                {
                    if (!context.GetIntType().Equals(operandExp.GetExpType()))
                    {
                        context.AddFatalError(A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly, exp.Operand);
                        return Error();
                    }

                    return Valid(new R.CallInternalUnaryOperatorExp(
                        R.InternalUnaryOperator.UnaryMinus_Int_Int,
                        operandExp, context.GetIntType()
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

    TranslationResult<R.Exp> VisitAssignBinaryOpExp(S.BinaryOpExp exp)
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

        var srcExpResult = ExpIR0ExpTranslator.Translate(exp.Operand1, context, destLocResult.LocType);
        if (!srcExpResult.IsValid(out var srcExp))
            return Error();

        var wrappedSrcExpResult = BodyMisc.CastExp_Exp(srcExp, destLocResult.LocType, exp, context);
        if (!wrappedSrcExpResult.IsValid(out var wrappedSrcExp))
            return Error();

        return Valid(new R.AssignExp(destLocResult.Loc, wrappedSrcExp));
    }

    public TranslationResult<R.Exp> TranslateBinaryOp(S.BinaryOpExp exp)
    {
        // 1. Assign 먼저 처리
        if (exp.Kind == S.BinaryOpKind.Assign)
        {
            return VisitAssignBinaryOpExp(exp);
        }

        var operandExpResult0 = ExpIR0ExpTranslator.Translate(exp.Operand0, context, hintType: null);
        if (!operandExpResult0.IsValid(out var operandExp0))
            return Error();

        var operandExpResult1 = ExpIR0ExpTranslator.Translate(exp.Operand1, context, hintType: null);
        if (!operandExpResult1.IsValid(out var operandExp1))
            return Error();

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
                    return Valid(new R.CallInternalUnaryOperatorExp(R.InternalUnaryOperator.LogicalNot_Bool_Bool, equalExp, info.ResultType));
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
                return Valid(new R.CallInternalBinaryOperatorExp(info.IR0Operator, castExp0, castExp1, info.ResultType));
            }
        }

        // Operator를 찾을 수 없습니다
        context.AddFatalError(A0802_BinaryOp_OperatorNotFound, exp);
        return Error();
    }

    public TranslationResult<R.Exp> TranslateLambda(S.LambdaExp expSyntax)
    {
        // TODO: 리턴 타입과 인자타입은 타입 힌트를 반영해야 한다
        IType? retType = null;

        var lambdaInfoResult = LambdaVisitor.Translate(retType, expSyntax.Params, expSyntax.Body, context, nodeForErrorReport: expSyntax);
        if (!lambdaInfoResult.IsValid(out var lambdaInfo))
            return Error();
        
        return Valid(new R.LambdaExp(lambdaInfo.Lambda, lambdaInfo.Args));
    }

    public TranslationResult<R.Exp> TranslateList(S.ListExp exp)
    {
        var builder = ImmutableArray.CreateBuilder<R.Exp>(exp.Elems.Length);

        // TODO: 타입 힌트도 이용해야 할 것 같다
        IType? curElemType = (exp.ElemType != null) ? context.MakeType(exp.ElemType) : null;

        foreach (var elem in exp.Elems)
        {
            var elemExpResult = ExpIR0ExpTranslator.Translate(elem, context, hintType: null);
            if (!elemExpResult.IsValid(out var elemExp))
                return Error();

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
                return Error();
            }
        }

        if (curElemType == null)
        {
            context.AddFatalError(A1701_ListExp_CantInferElementTypeWithEmptyElement, exp);
            return Error();
        }

        return Valid(new R.ListExp(builder.MoveToImmutable(), context.GetListType(curElemType)));
    }

    public TranslationResult<R.Exp> TranslateNew(S.NewExp exp) // throws ErrorCodeException
    {
        var classSymbol = context.MakeType(exp.Type) as ClassSymbol;
        if (classSymbol == null)
        {
            context.AddFatalError(A2601_NewExp_TypeIsNotClass, exp.Type);
            return Error();
        }

        // NOTICE: 생성자 검색 (AnalyzeCallExpTypeCallable 부분과 비슷)                
        var classDecl = classSymbol.GetDecl();

        var candidates = FuncsMatcherExtensions.MakeCandidates(classDecl.GetConstructorCount(), classDecl.GetConstructor);
        var matchResultEntries = FuncsMatcher.Match(context, classSymbol.GetTypeEnv(), candidates, exp.Args, partialTypeArgs: default);

        var matchCount = matchResultEntries.Length;
        if (matchCount == 1)
        {
            var matchResultEntry = matchResultEntries[0];
            var constructor = classSymbol.GetConstructor(matchResultEntry.Index);

            if (!context.CanAccess(constructor))
            {
                context.AddFatalError(A2011_ResolveIdentifier_TryAccessingPrivateMember, exp);
                return Error();
            }

            return Valid(new R.NewClassExp(constructor, matchResultEntry.Args));
        }
        else if (matchCount == 0)
        {
            context.AddFatalError(A2602_NewExp_NoMatchedClassConstructor, exp);
            return Error();
        }
        else
        {
            context.AddFatalError(A2603_NewExp_MultipleMatchedClassConstructors, exp);
            return Error();
        }
    }

    public TranslationResult<R.Exp> TranslateCall(S.CallExp exp)
    {
        var callableResult = ExpIntermediateExpTranslator.Translate(exp.Callable, context, hintType);
        if (!callableResult.IsValid(out var callable))
            return Error();

        return CallableAndArgsBinder.Bind(callable, exp.Args, context, exp, exp.Callable);
    }

    public TranslationResult<R.Exp> TranslateBox(S.BoxExp exp)
    {
        // hintType전수
        var innerHintType = (hintType as BoxPtrType)?.InnerType;

        var innerExpResult = ExpIR0ExpTranslator.Translate(exp.InnerExp, context, innerHintType);
        if (!innerExpResult.IsValid(out var innerExp))
            return Error();

        return Valid(new R.BoxExp(innerExp));
    }
}