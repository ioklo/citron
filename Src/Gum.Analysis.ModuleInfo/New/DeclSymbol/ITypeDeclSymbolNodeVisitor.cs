namespace Gum.Analysis
{
    public interface ITypeDeclSymbolNodeVisitor
    {
        void VisitClassDecl(ClassDeclSymbol classDecl);
        void VisitStructDecl(StructDeclSymbol structDecl);
        void VisitEnumDecl(EnumDeclSymbol enumDecl);
        void VisitEnumElemDecl(EnumElemDeclSymbol enumElemDecl);
    }
}