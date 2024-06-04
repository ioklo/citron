export module Citron.DeclSymbols:ModuleDeclSymbol;

import <string>;
import :TopLevelDeclSymbolComponent;

namespace Citron {

export class ModuleDeclSymbol
{
    std::string moduleName;
    bool bReference; // 지금 만들고 있는 모듈인지, 아닌지 여부를 판별할때 쓴다
    TopLevelDeclSymbolComponent topLevelComp;
};

    /*
    * 
    public class ModuleDeclSymbol
        : ITopLevelDeclSymbolNode
        , ITopLevelDeclContainable
        , ICyclicEqualityComparableClass<ModuleDeclSymbol>
        , ISerializable
    {
        Name moduleName;
        bool bReference;

        TopLevelDeclSymbolComponent topLevelComp;

        public ModuleDeclSymbol(Name moduleName, bool bReference)
        {
            this.moduleName = moduleName;
            this.bReference = bReference;

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

        public Name GetName()
        {
            return moduleName;
        }

        public DeclSymbolNodeName GetNodeName()
        {
            return new DeclSymbolNodeName(moduleName, 0, default);
        }

        public IDeclSymbolNode? GetOuterDeclNode()
        {
            return null;
        }

        TResult IDeclSymbolNode.Accept<TDeclSymbolNodeVisitor, TResult>(ref TDeclSymbolNodeVisitor visitor)
            => visitor.VisitModule(this);

        public Accessor GetAccessor()
        {
            return Accessor.Public;
        }

        public bool IsReference()
        {
            return bReference;
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

        public void AddType(ITypeDeclSymbol type)
            => topLevelComp.AddType(type);

        public void AddFunc(GlobalFuncDeclSymbol func)
            => topLevelComp.AddFunc(func);

        public void AddNamespace(NamespaceDeclSymbol ns)
            => topLevelComp.AddNamespace(ns);

        bool ICyclicEqualityComparableClass<ITopLevelDeclSymbolNode>.CyclicEquals(ITopLevelDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is ModuleDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<IDeclSymbolNode>.CyclicEquals(IDeclSymbolNode other, ref CyclicEqualityCompareContext context)
            => other is ModuleDeclSymbol otherDeclSymbol && CyclicEquals(otherDeclSymbol, ref context);

        bool ICyclicEqualityComparableClass<ModuleDeclSymbol>.CyclicEquals(ModuleDeclSymbol other, ref CyclicEqualityCompareContext context)
            => CyclicEquals(other, ref context);

        bool CyclicEquals(ModuleDeclSymbol other, ref CyclicEqualityCompareContext context)
        {
            if (!moduleName.Equals(other.moduleName))
                return false;

            if (!bReference.Equals(other.bReference))
                return false;

            if (!topLevelComp.CyclicEquals(ref other.topLevelComp, ref context))
                return false;

            return true;
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(moduleName), moduleName);
            context.SerializeBool(nameof(bReference), bReference);
            context.SerializeValueRef(nameof(topLevelComp), ref topLevelComp);
        }
    }
    */
}