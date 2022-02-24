using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using M = Citron.CompileTime;

namespace Citron.Analysis
{
    [AutoConstructor]
    public partial class EnumElemMemberVarDeclSymbol : IDeclSymbolNode
    {
        IHolder<EnumElemDeclSymbol> outerHolder;
        IHolder<ITypeSymbol> declTypeHolder;
        M.Name name;

        public M.Name GetName()
        {
            return name;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outerHolder.GetValue();
        }

        public ITypeSymbol GetDeclType()
        {
            return declTypeHolder.GetValue();
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitEnumElemMemberVar(this);
        }

        public M.AccessModifier GetAccessModifier()
        {
            return M.AccessModifier.Public;
        }
    }
}