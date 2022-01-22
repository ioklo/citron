namespace Gum.Analysis
{
    public interface ITypeDeclSymbolContainer
    {
        IDeclSymbolNode GetOuterDeclNode();
        void Apply(ITypeDeclSymbolVisitor visitor);
    }
}