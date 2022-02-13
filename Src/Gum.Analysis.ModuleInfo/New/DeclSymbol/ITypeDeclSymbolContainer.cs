using M = Gum.CompileTime;

namespace Citron.Analysis
{
    public interface ITypeDeclSymbolContainer
    {
        IDeclSymbolNode GetOuterDeclNode();
        void Apply(ITypeDeclSymbolVisitor visitor);
        M.AccessModifier GetAccessModifier();
    }
}