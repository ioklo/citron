namespace Citron.Symbol
{
    public interface ITypeVisitor<TResult>
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
        TResult VisitVar(VarType type);

        TResult VisitFunc(FuncType type);
        TResult VisitLocalPtr(LocalPtrType type);
        TResult VisitBoxPtr(BoxPtrType type);
    }

    public static class TypeVisitorExtensions
    {
        public static void Accept<TVisitor, TResult>(this IType type, ref TVisitor visitor)
            where TVisitor : struct, ITypeVisitor<TResult>
        {
            var typeImpl = (TypeImpl)type; // 강제 캐스팅
            typeImpl.Accept<TVisitor, TResult>(ref visitor);
        }
    }
}
