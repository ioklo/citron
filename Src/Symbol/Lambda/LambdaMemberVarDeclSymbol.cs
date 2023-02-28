using Citron.Collections;
using Citron.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Citron.Symbol
{   
    public class LambdaMemberVarDeclSymbol : IDeclSymbolNode, ICyclicEqualityComparableClass<LambdaMemberVarDeclSymbol>
    {   
        LambdaDeclSymbol outer;
        IType type;
        Name name;

        public LambdaMemberVarDeclSymbol(LambdaDeclSymbol outer, IType type, Name name)
        {
            this.outer = outer;
            this.type = type;
            this.name = name;
        }

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

        void IDeclSymbolNode.AcceptDeclSymbolVisitor<TDeclSymbolNodeVisitor>(ref TDeclSymbolNodeVisitor visitor)
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
