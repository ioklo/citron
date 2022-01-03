using Gum.Collections;

using M = Gum.CompileTime;

namespace Gum.Analysis
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

        public GlobalFuncDeclSymbol? GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return globalFuncDict.Get(new DeclSymbolNodeName(name, typeParamCount, paramTypes));
        }

        public IDeclSymbolNode? GetMemberDeclNode(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            if (paramTypes.IsEmpty)
            {
                if (typeParamCount == 0)
                {
                    var ns = namespaceDict.GetValueOrDefault(name);
                    if (ns != null) 
                        return ns;
                }

                var globalType = GetType(name, typeParamCount);
                if (globalType != null)
                    return globalType;
            }

            var globalFunc = GetFunc(name, typeParamCount, paramTypes);
            if (globalFunc != null)
                return globalFunc;

            return null;
        }
    }
}
