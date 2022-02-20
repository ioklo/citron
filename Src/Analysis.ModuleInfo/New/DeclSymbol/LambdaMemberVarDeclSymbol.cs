using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using M = Citron.CompileTime;

namespace Citron.Analysis
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

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, ImmutableArray<M.FuncParamId> paramIds)
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

        public ITypeSymbol GetDeclType()
        {
            return type;
        }

        public M.AccessModifier GetAccessModifier()
        {
            return M.AccessModifier.Private; // 
        }
    }
}
