using Citron.Infra;
using System.Collections.Generic;
using System.Linq;

namespace Citron.Symbol
{
    public class GlobalVarDeclSymbol : ITopLevelDeclSymbolNode, ICyclicEqualityComparableClass<GlobalVarDeclSymbol>
    {
        ITopLevelDeclSymbolNode outer;
        IType type;
        Name name;

        public GlobalVarDeclSymbol(ITopLevelDeclSymbolNode outer, IType type, Name name)
        {
            this.outer = outer;
            this.type = type;
            this.name = name;
        }

        void IDeclSymbolNode.Accept<TDeclSymbolNodeVisitor>(TDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitGlobalVar(this);
        }

        void IDeclSymbolNode.Accept<TDeclSymbolNodeVisitor>(ref TDeclSymbolNodeVisitor visitor)
        {
            visitor.VisitGlobalVar(this);
        }

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is GlobalVarDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<ITopLevelDeclSymbolNode>.CyclicEquals(ITopLevelDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is GlobalVarDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<GlobalVarDeclSymbol>.CyclicEquals(GlobalVarDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(GlobalVarDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(type, other.type))
                return false;

            if (!name.Equals(other.name))
                return false;

            return true;
        }

        Accessor IDeclSymbolNode.GetAccessor()
        {
            return Accessor.Private;
        }

        IEnumerable<IDeclSymbolNode> IDeclSymbolNode.GetMemberDeclNodes()
        {
            return Enumerable.Empty<IDeclSymbolNode>();
        }

        DeclSymbolNodeName IDeclSymbolNode.GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        IDeclSymbolNode? IDeclSymbolNode.GetOuterDeclNode()
        {
            return outer;
        }

        Name IDeclSymbolNode.GetTypeParam(int i)
        {
            throw new RuntimeFatalException();
        }

        int IDeclSymbolNode.GetTypeParamCount()
        {
            return 0;
        }
    }
}