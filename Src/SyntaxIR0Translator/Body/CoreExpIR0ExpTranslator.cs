using System;
using System.Diagnostics;

using Citron.Symbol;
using Citron.Collections;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis
{
    struct CoreExpIR0ExpTranslator
    {
        IType? hintType;
        ScopeContext context;

        public CoreExpIR0ExpTranslator(IType? hintType, ScopeContext context)
        {
            this.hintType = hintType;
            this.context = context;
        }

        public R.Exp TranslateNullLiteral(S.NullLiteralExp exp)
        {
            if (hintType != null)
            {
                // int? i = null;
                if (hintType is NullableType nullableHintType)
                {
                    return new R.NewNullableExp(null, nullableHintType);
                }
            }

            context.AddFatalError(A2701_NullLiteralExp_CantInferNullableType, exp);
            throw new UnreachableException();
        }

        public R.Exp TranslateBoolLiteral(S.BoolLiteralExp exp)
        {
            return new R.BoolLiteralExp(exp.Value, context.GetBoolType());
        }

        public R.Exp TranslateIntLiteral(S.IntLiteralExp exp)
        {
            return new R.IntLiteralExp(exp.Value, context.GetIntType());
        }

        R.StringExpElement VisitStringExpElement(S.StringExpElement elem)
        {
            var stringType = context.GetStringType();

            if (elem is S.ExpStringExpElement expElem)
            {
                var exp = ExpIR0ExpTranslator.Translate(expElem.Exp, context, null);
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

            throw new UnreachableException();
        }

        public R.Exp TranslateString(S.StringExp exp)
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

            return new R.StringExp(builder.ToImmutable(), context.GetStringType());
        }

        // int만 지원한다
        public R.Exp VisitIntUnaryAssignExp(S.Exp operand, R.InternalUnaryAssignOperator op)
        {
            var locResult = ExpIR0LocTranslator.Translate(operand, context, hintType: null, bWrapExpAsLoc: false);
            if (locResult != null)
            {
                var intType = context.GetIntType();

                // int type 검사, exact match
                if (!locResult.Value.Type.Equals(context.GetIntType()))
                    context.AddFatalError(A0601_UnaryAssignOp_IntTypeIsAllowedOnly, operand);

                return new R.CallInternalUnaryAssignOperatorExp(op, locResult.Value.Loc, intType);
            }
            else
            {
                context.AddFatalError(A0602_UnaryAssignOp_AssignableExpressionIsAllowedOnly, operand);
                throw new UnreachableException();
            }
        }


        public R.Exp TranslateUnaryOp(S.UnaryOpExp exp)
        {
            var operandExp = ExpIR0ExpTranslator.Translate(exp.Operand, context, hintType: null);

            switch (exp.Kind)
            {
                case S.UnaryOpKind.LogicalNot:
                    {
                        // exact match
                        if (!context.GetBoolType().Equals(operandExp.GetExpType()))
                            context.AddFatalError(A0701_UnaryOp_LogicalNotOperatorIsAppliedToBoolTypeOperandOnly, exp.Operand);

                        return new R.CallInternalUnaryOperatorExp(
                            R.InternalUnaryOperator.LogicalNot_Bool_Bool,
                            operandExp,
                            context.GetBoolType()
                        );
                    }

                case S.UnaryOpKind.Minus:
                    {
                        if (!context.GetIntType().Equals(operandExp.GetExpType()))
                            context.AddFatalError(A0702_UnaryOp_UnaryMinusOperatorIsAppliedToIntTypeOperandOnly, exp.Operand);

                        return new R.CallInternalUnaryOperatorExp(
                            R.InternalUnaryOperator.UnaryMinus_Int_Int,
                            operandExp, context.GetIntType()
                        );
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

        R.Exp VisitAssignBinaryOpExp(S.BinaryOpExp exp)
        {
            // syntax 에서는 exp로 보이지만, R로 변환할 경우 Location 명령이어야 한다
            var destLoc = ExpIR0LocTranslator.Translate(exp.Operand0, context, hintType: null, bWrapExpAsLoc: false);

            if (destLocResult != null)
            {
                // 안되는거 체크
                switch (destLocResult.Value.Loc)
                {
                    // int x = 0; var l = () { x = 3; }, TODO: 이거 가능하도록
                    case R.LambdaMemberVarLoc:
                        context.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
                        break;

                    case R.ThisLoc:          // this = x;
                        context.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
                        break;

                    case R.TempLoc:
                        context.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
                        break;
                }

                var srcExp = ExpIR0ExpTranslator.Translate(exp.Operand1, context, destLocResult.Value.Type);
                var wrappedSrcExp = BodyMisc.CastExp_Exp(srcExp, destLocResult.Value.Type, exp, context);

                return new R.AssignExp(destLocResult.Value.Loc, wrappedSrcExp);
            }
            else
            {
                context.AddFatalError(A0803_BinaryOp_LeftOperandIsNotAssignable, exp.Operand0);
                throw new UnreachableException();
            }
        }

        public R.Exp TranslateBinaryOp(S.BinaryOpExp exp)
        {
            // 1. Assign 먼저 처리
            if (exp.Kind == S.BinaryOpKind.Assign)
            {
                return VisitAssignBinaryOpExp(exp);
            }

            var operandExp0 = ExpIR0ExpTranslator.Translate(exp.Operand0, context, hintType: null);
            var operandExp1 = ExpIR0ExpTranslator.Translate(exp.Operand1, context, hintType: null);

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
                        return new R.CallInternalUnaryOperatorExp(R.InternalUnaryOperator.LogicalNot_Bool_Bool, equalExp, info.ResultType);
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
                    return new R.CallInternalBinaryOperatorExp(info.IR0Operator, castExp0, castExp1, info.ResultType);
                }
            }

            // Operator를 찾을 수 없습니다
            context.AddFatalError(A0802_BinaryOp_OperatorNotFound, exp);
            throw new UnreachableException();
        }

        public R.Exp TranslateLambda(S.LambdaExp exp)
        {
            // TODO: 리턴 타입과 인자타입은 타입 힌트를 반영해야 한다
            IType? retType = null;

            var visitor = new LambdaVisitor(retType, exp.Params, exp.Body, context, nodeForErrorReport: exp);
            var (lambdaSymbol, args) = visitor.Visit();
            return new R.LambdaExp(lambdaSymbol, args);
        }

        public R.Exp TranslateList(S.ListExp exp)
        {
            var builder = ImmutableArray.CreateBuilder<R.Exp>(exp.Elems.Length);

            // TODO: 타입 힌트도 이용해야 할 것 같다
            IType? curElemType = (exp.ElemType != null) ? context.MakeType(exp.ElemType) : null;

            foreach (var elem in exp.Elems)
            {
                var elemExp = ExpIR0ExpTranslator.Translate(elem, context, hintType: null);
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

            return new R.ListExp(builder.MoveToImmutable(), context.GetListType(curElemType));
        }

        public R.Exp TranslateNew(S.NewExp exp)
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

                    return new R.NewClassExp(constructor, successResult.Args);
            }

            throw new UnreachableException();
        }

        public R.Exp TranslateCall(S.CallExp exp)
        {
            var callable = ExpIntermediateExpTranslator.Translate(exp.Callable, context, hintType);
            return CallableAndArgsBinder.Bind(callable, exp.Args, context, exp, exp.Callable);
        }
    }
}