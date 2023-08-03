using System.Threading.Tasks;

namespace Citron.IR0;

public interface IIR0ExpVisitor<TResult>
{
    // Storage
    TResult VisitLoad(LoadExp exp);
    TResult VisitAssign(AssignExp exp);
    TResult VisitBoxExp(BoxExp exp);
    TResult VisitLocalRef(LocalRefExp exp);

    // Literal
    TResult VisitBoolLiteral(BoolLiteralExp exp);
    TResult VisitIntLiteral(IntLiteralExp exp);
    TResult VisitString(StringExp exp);

    // List
    TResult VisitList(ListExp exp);
    TResult VisitListIterator(ListIteratorExp exp);

    // Call Internal
    TResult VisitCallInternalUnaryOperator(CallInternalUnaryOperatorExp exp);
    TResult VisitCallInternalUnaryAssignOperator(CallInternalUnaryAssignOperatorExp exp);
    TResult VisitCallInternalBinaryOperator(CallInternalBinaryOperatorExp exp);

    // Global
    TResult VisitCallGlobalFunc(CallGlobalFuncExp exp);    

    // Class
    TResult VisitNewClass(NewClassExp exp);
    TResult VisitCallClassMemberFunc(CallClassMemberFuncExp exp);
    TResult VisitCastClass(CastClassExp exp);

    // Struct
    TResult VisitNewStruct(NewStructExp exp);
    TResult VisitCallStructMemberFunc(CallStructMemberFuncExp exp);

    // Enum
    TResult VisitNewEnumElem(NewEnumElemExp exp);
    TResult VisitCastEnumElemToEnum(CastEnumElemToEnumExp exp);

    // Nullable
    TResult VisitNewNullable(NewNullableExp exp);

    // Lambda
    TResult VisitLambda(LambdaExp exp);
    TResult VisitCallValue(CallValueExp exp);

    // Func
    TResult CastBoxedLambdaToFunc(CastBoxedLambdaToFuncExp castBoxedLambdaToFuncExp);
}
