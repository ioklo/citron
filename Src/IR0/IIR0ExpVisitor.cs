using System.Threading.Tasks;

namespace Citron.IR0;

public interface IIR0ExpVisitor<out TResult>
{
    // Storage
    TResult VisitLoad(LoadExp exp);
    TResult VisitAssign(AssignExp exp);

    // BoxRef
    TResult VisitBoxExp(BoxExp exp);
    TResult VisitStaticBoxRef(StaticBoxRefExp exp);
    TResult VisitClassMemberBoxRef(ClassMemberBoxRefExp exp);
    TResult VisitStructIndirectMemberBoxRef(StructIndirectMemberBoxRefExp exp);
    TResult VisitStructMemberBoxRef(StructMemberBoxRefExp exp);

    // LocalRef
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
    TResult VisitNullableNullLiteral(NullableNullLiteralExp exp);

    // Lambda
    TResult VisitLambda(LambdaExp exp);
    TResult VisitCallValue(CallLambdaExp exp);

    // Func
    TResult CastBoxedLambdaToFunc(CastBoxedLambdaToFuncExp exp);

    // InlineBlock
    TResult VisitInlineBlock(InlineBlockExp exp);

    // Test
    TResult VisitClassIsClassExp(ClassIsClassExp exp);
    TResult VisitClassAsClassExp(ClassAsClassExp exp);

    TResult VisitClassIsInterfaceExp(ClassIsInterfaceExp exp);
    TResult VisitClassAsInterfaceExp(ClassAsInterfaceExp exp);

    TResult VisitInterfaceIsClassExp(InterfaceIsClassExp exp);
    TResult VisitInterfaceAsClassExp(InterfaceAsClassExp exp);

    TResult VisitInterfaceIsInterfaceExp(InterfaceIsInterfaceExp exp);
    TResult VisitInterfaceAsInterfaceExp(InterfaceAsInterfaceExp exp);

    TResult VisitEnumIsEnumElemExp(EnumIsEnumElemExp exp);
    TResult VisitEnumAsEnumElemExp(EnumAsEnumElemExp exp);
    
}
