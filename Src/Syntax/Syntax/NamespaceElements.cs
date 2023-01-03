namespace Citron.Syntax
{
    public abstract record class NamespaceElement : ISyntaxNode
    {
        internal NamespaceElement() { }
    }

    public record class GlobalFuncDeclNamespaceElement(GlobalFuncDecl FuncDecl) : NamespaceElement;
    public record class NamespaceDeclNamespaceElement(NamespaceDecl NamespaceDecl) : NamespaceElement;
    public record class TypeDeclNamespaceElement(TypeDecl TypeDecl) : NamespaceElement;
}