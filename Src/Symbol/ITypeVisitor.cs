namespace Citron.Symbol
{
    public interface ITypeVisitor<out TResult>
    {
        TResult VisitEnumElem(EnumElemType type);
        TResult VisitEnum(EnumType type);
        TResult VisitClass(ClassType type);
        TResult VisitStruct(StructType type);
        TResult VisitInterface(InterfaceType type);

        TResult VisitNullable(NullableType type);
        TResult VisitTypeVar(TypeVarType type);
        TResult VisitVoid(VoidType type);
        TResult VisitLambda(LambdaType type);
        TResult VisitTuple(TupleType type);

        TResult VisitFunc(FuncType type);
        TResult VisitLocalPtr(LocalPtrType type);
        TResult VisitBoxPtr(BoxPtrType type);
    }
}
