namespace Citron.Analysis
{
    public interface ITypeDeclSymbolVisitor
    {
        void VisitClass(ClassDeclSymbol classDecl);
        void VisitStruct(StructDeclSymbol structDecl);
        void VisitEnum(EnumDeclSymbol enumDecl);
        void VisitEnumElem(EnumElemDeclSymbol enumElemDecl);
        void VisitInterface(InterfaceDeclSymbol interfaceDecl);
        void VisitLambda(LambdaDeclSymbol lambdaDecl);        
    }
}