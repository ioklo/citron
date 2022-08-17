using Citron.Module;

namespace Citron.Symbol
{
    public interface ITypeDeclSymbolContainer
    {
        IDeclSymbolNode GetOuterDeclNode();
        void Apply(ITypeDeclSymbolVisitor visitor);
        AccessModifier GetAccessModifier();
    }
}