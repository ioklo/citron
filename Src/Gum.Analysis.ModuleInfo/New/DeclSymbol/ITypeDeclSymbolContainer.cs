using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public interface ITypeDeclSymbolContainer
    {
        IDeclSymbolNode GetOuterDeclNode();
        void Apply(ITypeDeclSymbolVisitor visitor);
        M.AccessModifier GetAccessModifier();
    }
}