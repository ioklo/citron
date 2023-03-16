namespace Citron.IR0;

public interface IIR0ExpVisitor
{
    // Storage
    void VisitLoad(LoadExp loadExp);
    void VisitAssign(AssignExp assignExp);

    // Literal
    void VisitBoolLiteral(BoolLiteralExp boolExp);
    void VisitIntLiteral(IntLiteralExp intExp);
    void VisitString(StringExp stringExp);

    // List
    void VisitList(ListExp listExp);
    void VisitListIterator(ListIteratorExp listIterExp);

    // Call Internal
    void VisitCallInternalUnaryOperator(CallInternalUnaryOperatorExp ciuoExp);
    void VisitCallInternalUnaryAssignOperator(CallInternalUnaryAssignOperatorExp ciuaoExp);
    void VisitCallInternalBinaryOperator(CallInternalBinaryOperatorExp ciboExp);

    // Global
    void VisitCallGlobalFunc(CallGlobalFuncExp callFuncExp);    

    // Class
    void VisitNewClass(NewClassExp newClassExp);
    void VisitCallClassMemberFunc(CallClassMemberFuncExp callFuncExp);
    void VisitCastClass(CastClassExp castClassExp);

    // Struct
    void VisitNewStruct(NewStructExp newStructExp);
    void VisitCallStructMemberFunc(CallStructMemberFuncExp callFuncExp);

    // Enum
    void VisitNewEnumElem(NewEnumElemExp enumExp);
    void VisitCastEnumElemToEnum(CastEnumElemToEnumExp castEnumElemToEnumExp);

    // Nullable
    void VisitNewNullable(NewNullableExp newNullableExp);

    // Lambda
    void VisitLambda(LambdaExp lambdaExp);
    void VisitCallValue(CallValueExp callValueExp);
    void VisitCallFuncRef(CallFuncRefExp callFuncExp);
    void VisitCastFuncRef(CastFuncRefExp exp);
}
