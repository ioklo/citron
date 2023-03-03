using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol
{
    // ModuleDecl, NamespaceDecl에서 쓰이는 로직
    struct TopLevelDeclSymbolComponent : ICyclicEqualityComparableStruct<TopLevelDeclSymbolComponent>, ISerializable
    {
        List<NamespaceDeclSymbol> namespaceDecls;
        Dictionary<Name, NamespaceDeclSymbol> namespaceDict;

        TypeDeclSymbolComponent typeComp;
        FuncDeclSymbolComponent<GlobalFuncDeclSymbol> funcComp;

        public static TopLevelDeclSymbolComponent Make()
        {
            var comp = new TopLevelDeclSymbolComponent();
            comp.namespaceDecls = new List<NamespaceDeclSymbol>();
            comp.namespaceDict = new Dictionary<Name, NamespaceDeclSymbol>();
            comp.typeComp = TypeDeclSymbolComponent.Make();
            comp.funcComp = FuncDeclSymbolComponent.Make<GlobalFuncDeclSymbol>();
            return comp;
        }

        public ITypeDeclSymbol? GetType(Name name, int typeParamCount)
            => typeComp.GetType(name, typeParamCount);

        public void AddType(ITypeDeclSymbol decl)
            => typeComp.AddType(decl);

        public IEnumerable<GlobalFuncDeclSymbol> GetFuncs(Name name, int minTypeParamCount)
            => funcComp.GetFuncs(name, minTypeParamCount);

        public GlobalFuncDeclSymbol? GetFunc(Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
            => funcComp.GetFunc(name, typeParamCount, paramIds);

        public void AddFunc(GlobalFuncDeclSymbol decl)
            => funcComp.AddFunc(decl);

        public void AddNamespace(NamespaceDeclSymbol decl)
        {
            namespaceDecls.Add(decl);
            namespaceDict.Add(decl.GetName(), decl);
        }

        public NamespaceDeclSymbol? GetNamespace(Name name)
        {
            return namespaceDict.GetValueOrDefault(name);
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return namespaceDict.Values
                .Concat<IDeclSymbolNode>(namespaceDecls)
                .Concat(typeComp.GetEnumerable())
                .Concat(funcComp.GetEnumerable());
        }

        bool ICyclicEqualityComparableStruct<TopLevelDeclSymbolComponent>.CyclicEquals(ref TopLevelDeclSymbolComponent other, ref CyclicEqualityCompareContext context)
        {
            if (!namespaceDecls.CyclicEqualsClassItem(other.namespaceDecls, ref context))
                return false;

            if (!typeComp.CyclicEquals(ref other.typeComp, ref context))
                return false;

            if (!funcComp.CyclicEquals(ref other.funcComp, ref context))
                return false;

            return true;
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRefList(nameof(namespaceDecls), namespaceDecls);
            context.SerializeValueRef(nameof(typeComp), ref typeComp);
            context.SerializeValueRef(nameof(funcComp), ref funcComp);
        }
    }
}
