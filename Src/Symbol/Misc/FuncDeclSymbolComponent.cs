using Citron.Collections;
using Pretune;
using System.Collections.Generic;
using Citron.Module;
using System.Linq;
using System;

namespace Citron.Symbol
{
    public static class FuncDeclSymbolComponent
    {
        public static FuncDeclSymbolComponent<TFuncDeclSymbol> Make<TFuncDeclSymbol>()
            where TFuncDeclSymbol : IDeclSymbolNode
        {
            var funcDict = new FuncDeclSymbolComponent<TFuncDeclSymbol>();
            funcDict.map = new Dictionary<DeclSymbolNodeName, TFuncDeclSymbol>();
            funcDict.nameMap = new Dictionary<Name, List<TFuncDeclSymbol>>();
            return funcDict;
        }
    }

    [ExcludeComparison]
    public partial struct FuncDeclSymbolComponent<TFuncDeclSymbol>
        where TFuncDeclSymbol : IDeclSymbolNode
    {
        internal Dictionary<Name, List<TFuncDeclSymbol>> nameMap;
        internal Dictionary<DeclSymbolNodeName, TFuncDeclSymbol> map;

        // 기본 생성자를 호출하면 에러가 멤버 함수 호출시점에 나게 된다

        public void AddFunc(TFuncDeclSymbol decl)
        {
            var nodeName = decl.GetNodeName();

            map[nodeName] = decl;

            if (!nameMap.TryGetValue(nodeName.Name, out var list))
            {
                list = new List<TFuncDeclSymbol>();
                nameMap[nodeName.Name] = list;
            }
            list.Add(decl);
        }

        // typeParamCount는 total이 아니라 함수에 관해서다
        public IEnumerable<TFuncDeclSymbol> GetFuncs(Name name, int minTypeParamCount)
        {
            if (!nameMap.TryGetValue(name, out var list))
                yield break;

            foreach (var item in list)
            {
                if (minTypeParamCount <= item.GetTypeParamCount())
                    yield return item;
            }
        }

        public TFuncDeclSymbol? GetFunc(Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
        {
            return map.GetValueOrDefault(new DeclSymbolNodeName(name, typeParamCount, paramIds));
        }

        public IEnumerable<TFuncDeclSymbol> GetEnumerable()
        {
            return map.Values;
        }

        public IEnumerable<TFuncDeclSymbol> GetFuncs()
        {
            return map.Values;
        }
    }
}
