using Citron.Collections;
using Pretune;
using System.Collections.Generic;
using M = Citron.CompileTime;

namespace Citron.Analysis
{
    public class FuncDict
    {
        public static FuncDict<TFuncDeclSymbol> Build<TFuncDeclSymbol>(ImmutableArray<TFuncDeclSymbol> funcDecls)
            where TFuncDeclSymbol : IDeclSymbolNode
        {
            var funcsDict = new Dictionary<(M.Name Name, int TypeParamCount), List<TFuncDeclSymbol>>();
            var exactFuncsBuilder = ImmutableDictionary.CreateBuilder<DeclSymbolNodeName, TFuncDeclSymbol>();

            foreach (var funcDecl in funcDecls)
            {
                var funcName = funcDecl.GetNodeName();

                // mfunc의 typeparam이 n개면 n-1 .. 0 에도 다 넣는다
                for (int i = 0; i <= funcName.TypeParamCount; i++)
                {
                    var key = (funcName.Name, i);

                    if (!funcsDict.TryGetValue(key, out var list))
                    {
                        list = new List<TFuncDeclSymbol>();
                        funcsDict.Add(key, list);
                    }

                    list.Add(funcDecl);
                }

                exactFuncsBuilder.Add(funcName, funcDecl);
            }

            // funcsDict제작이 다 끝났으면
            var builder = ImmutableDictionary.CreateBuilder<(M.Name Name, int TypeParamCount), ImmutableArray<TFuncDeclSymbol>>();
            foreach (var keyValue in funcsDict)
                builder.Add(keyValue.Key, keyValue.Value.ToImmutableArray());

            var funcDict = new FuncDict<TFuncDeclSymbol>();
            funcDict.funcs = builder.ToImmutable();
            funcDict.exactFuncs = exactFuncsBuilder.ToImmutable();
            return funcDict;
        }
    }

    [ExcludeComparison]
    public partial struct FuncDict<TFuncDeclSymbol>
        where TFuncDeclSymbol : IDeclSymbolNode
    {
        internal ImmutableDictionary<(M.Name Name, int TypeParamCount), ImmutableArray<TFuncDeclSymbol>> funcs;
        internal ImmutableDictionary<DeclSymbolNodeName, TFuncDeclSymbol> exactFuncs;
        
        public ImmutableArray<TFuncDeclSymbol> Get(M.Name name, int minTypeParamCount)
        {
            return funcs.GetValueOrDefault((name, minTypeParamCount));
        }

        public TFuncDeclSymbol? Get(DeclSymbolNodeName name)
        {
            return exactFuncs.GetValueOrDefault(name);
        }

        public IEnumerable<TFuncDeclSymbol> GetEnumerable()
        {
            return exactFuncs.Values;
        }
    }
}
