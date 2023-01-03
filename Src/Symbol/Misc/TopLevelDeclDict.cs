using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Citron.Collections;

using Citron.Module;

namespace Citron.Symbol
{
    // ModuleDecl, NamespaceDecl에서 쓰이는 로직
    struct TopLevelDeclSymbolComponent
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
    }
}
