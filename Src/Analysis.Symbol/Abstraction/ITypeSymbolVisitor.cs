namespace Citron.Analysis
{
    public interface ITypeSymbolVisitor
    {
        void VisitEnumElem(EnumElemSymbol symbol);
        void VisitClass(ClassSymbol symbol);
        void VisitEnum(EnumSymbol symbol);
        void VisitInterface(InterfaceSymbol symbol);
        void VisitStruct(StructSymbol symbol);
        void VisitVoid(VoidSymbol symbol);
        void VisitTuple(TupleSymbol symbol);
        void VisitNullable(NullableSymbol symbol);
        void VisitLambda(LambdaSymbol lambdaSymbol);

        // 런타임이 만들어 지기 전까지 쓰일 Proxy 심볼
        void VisitInt(IntSymbol intSymbol);
    }
}