using R = Gum.IR0;

namespace Gum.IR0Visitor
{
    public interface IIR0ExpVisitor
    {
        void VisitLoadExp(R.LoadExp loadExp);
        void VisitStringExp(R.StringExp stringExp);
        void VisitIntLiteralExp(R.IntLiteralExp intExp);
        void VisitBoolLiteralExp(R.BoolLiteralExp boolExp);
        void VisitCallInternalUnaryOperatorExp(R.CallInternalUnaryOperatorExp ciuoExp);
        void VisitCallInternalUnaryAssignOperatorExp(R.CallInternalUnaryAssignOperatorExp ciuaoExp);
        void VisitCallInternalBinaryOperatorExp(R.CallInternalBinaryOperatorExp ciboExp);
        void VisitAssignExp(R.AssignExp assignExp);
        void VisitCallFuncExp(R.CallFuncExp callFuncExp);
        void VisitCallSeqFuncExp(R.CallSeqFuncExp callSeqFuncExp);
        void VisitCallValueExp(R.CallValueExp callValueExp);
        void VisitLambdaExp(R.LambdaExp lambdaExp);
        void VisitListExp(R.ListExp listExp);
        void VisitListIteratorExp(R.ListIteratorExp listIterExp);
        void VisitNewEnumElemExp(R.NewEnumElemExp enumExp);
        void VisitNewStructExp(R.NewStructExp newStructExp);
        void VisitNewClassExp(R.NewClassExp newClassExp);
        void VisitNewNullableExp(R.NewNullableExp newNullableExp);
        void VisitCastEnumElemToEnumExp(R.CastEnumElemToEnumExp castEnumElemToEnumExp);
        void VisitCastClassExp(R.CastClassExp castClassExp);
    }

    public interface IIR0ExpVisitor<TArg0>
    {
        void VisitLoadExp(R.LoadExp loadExp, TArg0 arg0);
        void VisitStringExp(R.StringExp stringExp, TArg0 arg0);
        void VisitIntLiteralExp(R.IntLiteralExp intExp, TArg0 arg0);
        void VisitBoolLiteralExp(R.BoolLiteralExp boolExp, TArg0 arg0);
        void VisitCallInternalUnaryOperatorExp(R.CallInternalUnaryOperatorExp ciuoExp, TArg0 arg0);
        void VisitCallInternalUnaryAssignOperatorExp(R.CallInternalUnaryAssignOperatorExp ciuaoExp, TArg0 arg0);
        void VisitCallInternalBinaryOperatorExp(R.CallInternalBinaryOperatorExp ciboExp, TArg0 arg0);
        void VisitAssignExp(R.AssignExp assignExp, TArg0 arg0);
        void VisitCallFuncExp(R.CallFuncExp callFuncExp, TArg0 arg0);
        void VisitCallSeqFuncExp(R.CallSeqFuncExp callSeqFuncExp, TArg0 arg0);
        void VisitCallValueExp(R.CallValueExp callValueExp, TArg0 arg0);
        void VisitLambdaExp(R.LambdaExp lambdaExp, TArg0 arg0);
        void VisitListExp(R.ListExp listExp, TArg0 arg0);
        void VisitListIteratorExp(R.ListIteratorExp listIterExp, TArg0 arg0);
        void VisitNewEnumElemExp(R.NewEnumElemExp enumExp, TArg0 arg0);
        void VisitNewStructExp(R.NewStructExp newStructExp, TArg0 arg0);
        void VisitNewClassExp(R.NewClassExp newClassExp, TArg0 arg0);
        void VisitNewNullableExp(R.NewNullableExp newNullableExp, TArg0 arg0);
        void VisitCastEnumElemToEnumExp(R.CastEnumElemToEnumExp castEnumElemToEnumExp, TArg0 arg0);
        void VisitCastClassExp(R.CastClassExp castClassExp, TArg0 arg0);
    }
}
