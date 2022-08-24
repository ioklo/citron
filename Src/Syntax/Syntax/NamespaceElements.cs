namespace Citron.Syntax
{
    public abstract record NamespaceElement : ISyntaxNode
    {
        internal NamespaceElement() { }
    }

    public record GlobalFuncDeclNamespaceElement(GlobalFuncDecl FuncDecl) : NamespaceElement;
    public record NamespaceDeclNamespaceElement(NamespaceDecl NamespaceDecl) : NamespaceElement;
    public record TypeDeclNamespaceElement(TypeDecl TypeDecl) : NamespaceElement;
}