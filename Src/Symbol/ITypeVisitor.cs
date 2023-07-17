namespace Citron.Symbol
{
    public interface ITypeVisitor
    {
        void VisitEnumElem(EnumElemType type);
        void VisitEnum(EnumType type);
        void VisitClass(ClassType type);
        void VisitStruct(StructType type);
        void VisitInterface(InterfaceType type);

        void VisitNullable(NullableType type);
        void VisitTypeVar(TypeVarType type);
        void VisitVoid(VoidType type);
        void VisitLambda(LambdaType type);
        void VisitTuple(TupleType type);
        void VisitVar(VarType type);

        void VisitFunc(FuncType type);
        void VisitLocalPtr(LocalPtrType type);
        void VisitBoxPtr(BoxPtrType type);
    }

    public static class TypeVisitorExtensions
    {
        public static void Accept<TVisitor>(this IType type, ref TVisitor visitor)
            where TVisitor : struct, ITypeVisitor
        {
            var typeImpl = (TypeImpl)type; // 강제 캐스팅
            typeImpl.Accept(ref visitor);
        }
    }
}
