using Citron.Collections;
using Citron.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Symbol
{
    public class NamespaceDeclSymbol 
        : ITopLevelDeclSymbolNode
        , ITopLevelDeclContainable
        , ICyclicEqualityComparableClass<NamespaceDeclSymbol>
        , ISerializable
    {
        ITopLevelDeclSymbolNode outer;
        Name name;

        TopLevelDeclSymbolComponent topLevelComp;

        public NamespaceDeclSymbol(ITopLevelDeclSymbolNode outer, Name name)
        {
            this.outer = outer;
            this.name = name;
            this.topLevelComp = TopLevelDeclSymbolComponent.Make();
        }

        int IDeclSymbolNode.GetTypeParamCount()
        {
            return 0;
        }

        Name IDeclSymbolNode.GetTypeParam(int i)
        {
            throw new RuntimeFatalException();
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {   
            return outer;
        }

        public Name GetName()
        {
            return name;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(name, 0, default);
        }

        TResult IDeclSymbolNode.Accept<TDeclSymbolNodeVisitor, TResult>(ref TDeclSymbolNodeVisitor visitor)
            => visitor.VisitNamespace(this);

        public Accessor GetAccessor()
        {
            return Accessor.Public; // TODO: private으로 지정할 수 있을까
        }

        public NamespaceDeclSymbol? GetNamespace(Name name)
            => topLevelComp.GetNamespace(name);

        public ITypeDeclSymbol? GetType(Name name, int typeParamCount)
           => topLevelComp.GetType(name, typeParamCount);

        public GlobalFuncDeclSymbol? GetFunc(Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
            => topLevelComp.GetFunc(name, typeParamCount, paramIds);

        public IEnumerable<GlobalFuncDeclSymbol> GetFuncs(Name name, int minTypeParamCount)
            => topLevelComp.GetFuncs(name, minTypeParamCount);

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
            => topLevelComp.GetMemberDeclNodes();

        public void AddNamespace(NamespaceDeclSymbol declSymbol)
            => topLevelComp.AddNamespace(declSymbol);

        public void AddType(ITypeDeclSymbol typeDeclSymbol)
            => topLevelComp.AddType(typeDeclSymbol);

        public void AddFunc(GlobalFuncDeclSymbol funcDeclSymbol)
            => topLevelComp.AddFunc(funcDeclSymbol);

        bool ICyclicEqualityComparableClass<ITopLevelDeclSymbolNode>.CyclicEquals(ITopLevelDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is NamespaceDeclSymbol nsDeclSymbolOther && CyclicEquals(nsDeclSymbolOther, ref context);

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is NamespaceDeclSymbol nsDeclSymbolOther && CyclicEquals(nsDeclSymbolOther, ref context);

        bool ICyclicEqualityComparableClass<NamespaceDeclSymbol>.CyclicEquals(NamespaceDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);
        
        bool CyclicEquals(NamespaceDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!context.CompareClass(outer, other.outer))
                return false;

            if (name.Equals(other.name))
                return false;

            if (topLevelComp.CyclicEquals(ref other.topLevelComp, ref context))
                return false;

            return true;
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(outer), outer);
            context.SerializeRef(nameof(name), name);
            context.SerializeValueRef(nameof(topLevelComp), ref topLevelComp);
        }
    }
}
