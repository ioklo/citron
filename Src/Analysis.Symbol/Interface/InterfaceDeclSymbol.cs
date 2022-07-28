﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citron.Collections;
using Citron.Infra;
using Pretune;

using M = Citron.CompileTime;

namespace Citron.Analysis
{
    [AutoConstructor]
    public partial class InterfaceDeclSymbol : ITypeDeclSymbol
    {
        IHolder<ITypeDeclSymbolContainer> containerHolder;

        M.Name name;
        ImmutableArray<string> typeParams;

        public void Apply(ITypeDeclSymbolVisitor visitor)
        {
            visitor.VisitInterface(this);
        }
        
        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            // TODO: 아직 없는 것이다
            return Enumerable.Empty<IDeclSymbolNode>();
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, typeParams.Length, default);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return containerHolder.GetValue().GetOuterDeclNode();
        }

        public M.Name GetName()
        {
            return name;
        }

        public void Apply(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitInterface(this);
        }

        public M.AccessModifier GetAccessModifier()
        {
            return containerHolder.GetValue().GetAccessModifier();
        }
    }
}