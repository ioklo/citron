namespace Citron.Symbol
{
    public interface ITypeDeclSymbolVisitor
    {
        void VisitClass(ClassDeclSymbol declSymbol);
        void VisitStruct(StructDeclSymbol declSymbol);
        void VisitEnum(EnumDeclSymbol declSymbol);
        void VisitEnumElem(EnumElemDeclSymbol declSymbol);        
        void VisitInterface(InterfaceDeclSymbol declSymbol);

        void VisitLambda(LambdaDeclSymbol declSymbol);
        void VisitLambdaMemberVar(LambdaMemberVarDeclSymbol declSymbol);


    }
}