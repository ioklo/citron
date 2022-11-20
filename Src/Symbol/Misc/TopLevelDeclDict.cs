using System;
using System.Collections.Generic;
using System.Linq;
using Citron.Collections;

using Citron.Module;

namespace Citron.Symbol
{
    // ModuleDecl, NamespaceDecl에서 쓰이는 로직
    struct TopLevelDeclDict
    {
        ImmutableDictionary<Name, NamespaceDeclSymbol> namespaceDict;
        TypeDict globalTypeDict;
        Lazy<FuncDict<GlobalFuncDeclSymbol>> lazyGlobalFuncDict;        

        public TopLevelDeclDict(ImmutableArray<NamespaceDeclSymbol> namespaces, ImmutableArray<ITypeDeclSymbol> types, ImmutableArray<GlobalFuncDeclSymbol> funcs)
        {
            var builder = ImmutableDictionary.CreateBuilder<Name, NamespaceDeclSymbol>();
            foreach (var ns in namespaces)
                builder.Add(ns.GetName(), ns);

            this.namespaceDict = builder.ToImmutable();
            this.globalTypeDict = TypeDict.Build(typeVars: default, types); // TopLevel에는 TypeVar가 없다
            this.lazyGlobalFuncDict = new Lazy<FuncDict<GlobalFuncDeclSymbol>>(() => FuncDict.Build(funcs));
        }

        public ImmutableArray<GlobalFuncDeclSymbol> GetFuncs(Name name, int minTypeParamCount)
        {
            return lazyGlobalFuncDict.Value.Get(name, minTypeParamCount);
        }

        public ITypeDeclSymbol? GetType(Name name, int typeParamCount)
        {
            return globalTypeDict.Get(new DeclSymbolNodeName(name, typeParamCount, default));
        }

        public GlobalFuncDeclSymbol? GetFunc(Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return lazyGlobalFuncDict.Value.Get(new DeclSymbolNodeName(name, typeParamCount, paramIds));
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return namespaceDict.Values.OfType<IDeclSymbolNode>()
                .Concat(globalTypeDict.GetEnumerable())
                .Concat(lazyGlobalFuncDict.Value.GetEnumerable());
        }
    }
}
