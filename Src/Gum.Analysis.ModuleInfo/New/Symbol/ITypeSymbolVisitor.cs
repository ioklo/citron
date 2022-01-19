namespace Gum.Analysis
{
    public interface ITypeSymbolVisitor
    {
        void VisitEnumElem(EnumElemSymbol symbol);
        void VisitClass(ClassSymbol symbol);
        void VisitEnum(EnumSymbol symbol);
        void VisitInterface(InterfaceSymbol symbol);
        void VisitStruct(StructSymbol symbol);
        void VisitVoid(VoidSymbol symbol);
        void VisitLambda(LambdaSymbol symbol);
    }
}