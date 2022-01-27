using Gum.Collections;
using Gum.Infra;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor]
    public partial class LambdaMemberVarDeclSymbol : IDeclSymbolNode
    {
        IHolder<LambdaDeclSymbol> outerHolder;
        ITypeSymbol type;
        M.Name name;

        public M.Name GetName()
        {
            return name;
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitLambdaMemberVar(this);
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return null;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outerHolder.GetValue();
        }

        public M.AccessModifier GetAccessModifier()
        {
            return M.AccessModifier.Private; // 
        }
    }
}
