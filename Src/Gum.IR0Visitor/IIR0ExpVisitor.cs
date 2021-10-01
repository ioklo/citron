using Gum.Infra;
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
}
