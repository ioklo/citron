using System.Collections.Generic;
using System.Linq;
using Citron.Collections;

using M = Citron.CompileTime;

namespace Citron.Analysis
{
    // ModuleDecl, NamespaceDecl에서 쓰이는 로직
    struct TopLevelDeclDict
    {
        ImmutableDictionary<M.Name, NamespaceDeclSymbol> namespaceDict;
        TypeDict<GlobalTypeDeclSymbol> globalTypeDict;
        FuncDict<GlobalFuncDeclSymbol> globalFuncDict;        

        public TopLevelDeclDict(ImmutableArray<NamespaceDeclSymbol> namespaces, ImmutableArray<GlobalTypeDeclSymbol> types, ImmutableArray<GlobalFuncDeclSymbol> funcs)
        {
            var builder = ImmutableDictionary.CreateBuilder<M.Name, NamespaceDeclSymbol>();
            foreach (var ns in namespaces)
                builder.Add(ns.GetName(), ns);

            this.namespaceDict = builder.ToImmutable();
            this.globalTypeDict = TypeDict.Build(types);
            this.globalFuncDict = FuncDict.Build(funcs);
        }

        public ImmutableArray<GlobalFuncDeclSymbol> GetFuncs(M.Name name, int minTypeParamCount)
        {
            return globalFuncDict.Get(name, minTypeParamCount);
        }

        public GlobalTypeDeclSymbol? GetType(M.Name name, int typeParamCount)
        {
            return globalTypeDict.Get(new DeclSymbolNodeName(name, typeParamCount, default));
        }

        public GlobalFuncDeclSymbol? GetFunc(M.Name name, int typeParamCount, ImmutableArray<M.FuncParamId> paramIds)
        {
            return globalFuncDict.Get(new DeclSymbolNodeName(name, typeParamCount, paramIds));
        }

        public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
        {
            return namespaceDict.Values.OfType<IDeclSymbolNode>()
                .Concat(globalTypeDict.GetEnumerable().Select(globalType => globalType.GetNode()))
                .Concat(globalFuncDict.GetEnumerable());
        }
    }
}
