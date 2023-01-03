namespace Citron.Symbol
{
    public interface ITypeSymbolVisitor
    {
        void VisitEnumElem(EnumElemSymbol symbol);
        void VisitClass(ClassSymbol symbol);
        void VisitEnum(EnumSymbol symbol);
        void VisitInterface(InterfaceSymbol symbol);
        void VisitStruct(StructSymbol symbol);
    }
}