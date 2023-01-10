﻿using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using Citron.Module;

namespace Citron.Symbol
{
    [AutoConstructor]
    public partial class LambdaMemberVarDeclSymbol : IDeclSymbolNode, ICyclicEqualityComparableClass<LambdaMemberVarDeclSymbol>
    {
        LambdaDeclSymbol outer;
        IType type;
        Name name;

        public Name GetName()
        {
            return name;
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
        }
        
        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        public IDeclSymbolNode GetOuterDeclNode()
        {
            return outer;
        }

        public IType GetDeclType()
        {
            return type;
        }

        public Accessor GetAccessor()
        {
            return Accessor.Private; // 
        }

        public void Accept(IDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitLambdaMemberVar(this);
        }

        int IDeclSymbolNode.GetTypeParamCount()
        {
            return 0;
        }

        Name IDeclSymbolNode.GetTypeParam(int i)
        {
            throw new RuntimeFatalException();
        }

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is LambdaMemberVarDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<LambdaMemberVarDeclSymbol>.CyclicEquals(LambdaMemberVarDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(LambdaMemberVarDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!context.CompareClass(type, other.type))
                return false;

            if (!name.Equals(other.name))
                return false;

            return true;
        }
    }
}
