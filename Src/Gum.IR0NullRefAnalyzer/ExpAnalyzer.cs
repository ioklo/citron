using Gum.Infra;
using Gum.IR0Visitor;

using R = Gum.IR0;

namespace Gum.IR0Analyzer.NullRefAnalysis
{
    struct ExpAnalyzer : IIR0ExpVisitor<AbstractValue>
    {
        LocalContext context;

        public static void Analyze(R.Exp exp, AbstractValue result, LocalContext context)
        {
            var expAnalyzer = new ExpAnalyzer(context);

            // call extension
            expAnalyzer.Visit(exp, result);
        }

        ExpAnalyzer(LocalContext context)
        {
            this.context = context;
        }

        public void VisitAssignExp(R.AssignExp assignExp, AbstractValue result)
        {
            var destValue = LocAnalyzer.Analyze(assignExp.Dest, context);            
            this.Visit(assignExp.Src, destValue);

            result.Set(destValue);
        }

        public void VisitBoolLiteralExp(R.BoolLiteralExp boolExp, AbstractValue value)
        {
            value.SetNotNull();
        }
        
        public void VisitCallFuncExp(R.CallFuncExp callFuncExp, AbstractValue value)
        {
            // unknown으로 지정하지만, value가 null-disallow라면 notnull로 세팅된다
            value.SetUnknown();
        }

        public void VisitCallInternalBinaryOperatorExp(R.CallInternalBinaryOperatorExp ciboExp, AbstractValue value)
        {
            switch(ciboExp.Operator)
            {
                case R.InternalBinaryOperator.Multiply_Int_Int_Int:
                case R.InternalBinaryOperator.Divide_Int_Int_Int:
                case R.InternalBinaryOperator.Modulo_Int_Int_Int:
                case R.InternalBinaryOperator.Add_Int_Int_Int:
                case R.InternalBinaryOperator.Add_String_String_String:
                case R.InternalBinaryOperator.Subtract_Int_Int_Int:
                case R.InternalBinaryOperator.LessThan_Int_Int_Bool:
                case R.InternalBinaryOperator.LessThan_String_String_Bool:
                case R.InternalBinaryOperator.GreaterThan_Int_Int_Bool:
                case R.InternalBinaryOperator.GreaterThan_String_String_Bool:
                case R.InternalBinaryOperator.LessThanOrEqual_Int_Int_Bool:
                case R.InternalBinaryOperator.LessThanOrEqual_String_String_Bool:
                case R.InternalBinaryOperator.GreaterThanOrEqual_Int_Int_Bool:
                case R.InternalBinaryOperator.GreaterThanOrEqual_String_String_Bool:
                case R.InternalBinaryOperator.Equal_Int_Int_Bool:
                case R.InternalBinaryOperator.Equal_Bool_Bool_Bool:
                case R.InternalBinaryOperator.Equal_String_String_Bool:
                    value.SetNotNull();
                    break;

                default:
                    throw new UnreachableCodeException();
            }            
        }

        public void VisitCallInternalUnaryAssignOperatorExp(R.CallInternalUnaryAssignOperatorExp ciuaoExp, AbstractValue value)
        {
            switch(ciuaoExp.Operator)
            {
                case R.InternalUnaryAssignOperator.PrefixInc_Int_Int:
                case R.InternalUnaryAssignOperator.PrefixDec_Int_Int:
                case R.InternalUnaryAssignOperator.PostfixInc_Int_Int:
                case R.InternalUnaryAssignOperator.PostfixDec_Int_Int:
                    value.SetNotNull();
                    break;

                default:
                    throw new UnreachableCodeException();
            }
        }

        public void VisitCallInternalUnaryOperatorExp(R.CallInternalUnaryOperatorExp ciuoExp, AbstractValue value)
        {
            switch(ciuoExp.Operator)
            {
                case R.InternalUnaryOperator.LogicalNot_Bool_Bool:
                case R.InternalUnaryOperator.UnaryMinus_Int_Int:
                
                case R.InternalUnaryOperator.ToString_Bool_String:
                case R.InternalUnaryOperator.ToString_Int_String:
                    value.SetNotNull();
                    break;

                default:
                    throw new UnreachableCodeException();
            }
        }

        public void VisitCallSeqFuncExp(R.CallSeqFuncExp callSeqFuncExp, AbstractValue value)
        {
            value.SetNotNull();
        }

        public void VisitCallValueExp(R.CallValueExp callValueExp, AbstractValue value)
        {
            value.SetUnknown();
        }

        // (C)b;
        public void VisitCastClassExp(R.CastClassExp castClassExp, AbstractValue value)
        {
            // B? b;
            // (C)b; 이런건 없다
            value.SetNotNull();
        }

        public void VisitCastEnumElemToEnumExp(R.CastEnumElemToEnumExp castEnumElemToEnumExp, AbstractValue value)
        {
            value.SetNotNull();
        }

        public void VisitIntLiteralExp(R.IntLiteralExp intExp, AbstractValue value)
        {
            value.SetNotNull();
        }

        public void VisitLambdaExp(R.LambdaExp lambdaExp, AbstractValue value)
        {
            value.SetNotNull();
        }

        public void VisitListExp(R.ListExp listExp, AbstractValue value)
        {
            value.SetNotNull();
        }

        public void VisitListIteratorExp(R.ListIteratorExp listIterExp, AbstractValue value)
        {
            value.SetNotNull();
        }

        public void VisitLoadExp(R.LoadExp loadExp, AbstractValue value)
        {
            var srcValue = LocAnalyzer.Analyze(loadExp.Loc, context);
            value.Set(srcValue);
        }

        public void VisitNewClassExp(R.NewClassExp newClassExp, AbstractValue value)
        {
            value.SetNotNull();
        }

        public void VisitNewEnumElemExp(R.NewEnumElemExp enumExp, AbstractValue value)
        {
            value.SetNotNull();
        }

        public void VisitNewNullableExp(R.NewNullableExp newNullableExp, AbstractValue value)
        {
            if (newNullableExp.ValueExp != null)
                value.SetNotNull();
            else
                value.SetNull();
        }

        public void VisitNewStructExp(R.NewStructExp newStructExp, AbstractValue value)
        {
            value.SetNotNull();
        }

        public void VisitStringExp(R.StringExp stringExp, AbstractValue value)
        {
            value.SetNotNull();
        }
    }
}