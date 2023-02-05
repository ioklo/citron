using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citron.Collections;
using Citron.Infra;
using Pretune;


namespace Citron.Symbol
{
    [AutoConstructor]
    public partial class InterfaceDeclSymbol : ITypeDeclSymbol, ICyclicEqualityComparableClass<InterfaceDeclSymbol>
    {
        IDeclSymbolNode outer;
        Accessor accessModifier;

        Name name;
        ImmutableArray<Name> typeParams;

        public int GetTypeParamCount()
        {
            return typeParams.Length;
        }

        public Name GetTypeParam(int i)
        {
            return typeParams[i];
        }

        int IDeclSymbolNode.GetTypeParamCount()
            => GetTypeParamCount();

        Name IDeclSymbolNode.GetTypeParam(int i)
            => GetTypeParam(i);

        public void Accept(ITypeDeclSymbolVisitor visitor)
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
            return outer;
        }

        public Name GetName()
        {
            return name;
        }

        void IDeclSymbolNode.Accept<TDeclSymbolNodeVisitor>(TDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitInterface(this);
        }

        void IDeclSymbolNode.Accept<TDeclSymbolNodeVisitor>(ref TDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitInterface(this);
        }

        public Accessor GetAccessor()
        {
            return accessModifier;
        }

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is InterfaceDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<ITypeDeclSymbol>.CyclicEquals(ITypeDeclSymbol other, ref CyclicEqualityCompareContext context)
            => other is InterfaceDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<InterfaceDeclSymbol>.CyclicEquals(InterfaceDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(InterfaceDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (!accessModifier.Equals(other.accessModifier))
                return false;

            if (!name.Equals(other.name))
                return false;

            if (!typeParams.Equals(typeParams))
                return false;

            return true;
        }
    }
}
