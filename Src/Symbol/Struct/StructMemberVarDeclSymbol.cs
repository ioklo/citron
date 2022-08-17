using System;
using System.Collections.Generic;
using System.Linq;
using Citron.Collections;
using Citron.Infra;
using Pretune;

using Citron.Module;

namespace Citron.Symbol
{
    [AutoConstructor]
    public partial class StructMemberVarDeclSymbol : IDeclSymbolNode
    {
        IHolder<StructDeclSymbol> outer;

        AccessModifier accessModifier;
        bool bStatic;
        IHolder<ITypeSymbol> declTypeHolder;
        Name name;

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return outer.GetValue();
        }
        
        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
        }

        public Name GetName()
        {
            return name;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public ITypeSymbol GetDeclType()
        {
            return declTypeHolder.GetValue();
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitStructMemberVar(this);
        }

        public bool IsStatic()
        {
            return bStatic;
        }

        public AccessModifier GetAccessModifier()
        {
            return accessModifier;
        }
    }
}