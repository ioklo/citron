using Gum.Collections;
using Pretune;
using System.Collections.Generic;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [ExcludeComparison]
    public partial struct ModuleFuncDict
    {
        ImmutableDictionary<(M.Name Name, int TypeParamCount), ImmutableArray<IModuleFuncInfo>> funcs;
        ImmutableDictionary<(M.Name Name, int TypeParamCount, M.ParamTypes ParamTypes), IModuleFuncInfo> exactFuncs;

        public ModuleFuncDict(ImmutableArray<IModuleFuncInfo> funcInfos)
        {
            var funcsDict = new Dictionary<(M.Name Name, int TypeParamCount), List<IModuleFuncInfo>>();
            var exactFuncsBuilder = ImmutableDictionary.CreateBuilder<(M.Name Name, int TypeParamCount, M.ParamTypes paramTypes), IModuleFuncInfo>();

            foreach (var func in funcInfos)
            {
                var funcName = func.GetName();
                var typeParamCount = func.GetTypeParams().Length;

                // mfunc의 typeparam이 n개면 n-1 .. 0 에도 다 넣는다
                for (int i = 0; i <= typeParamCount; i++)
                {
                    var key = (funcName, i);

                    if (!funcsDict.TryGetValue(key, out var list))
                    {
                        list = new List<IModuleFuncInfo>();
                        funcsDict.Add(key, list);
                    }

                    list.Add(func);
                }

                var paramTypes = func.GetParamTypes();
                exactFuncsBuilder.Add((funcName, typeParamCount, paramTypes), func);
            }

            // funcsDict제작이 다 끝났으면
            var builder = ImmutableDictionary.CreateBuilder<(M.Name Name, int TypeParamCount), ImmutableArray<IModuleFuncInfo>>();
            foreach (var keyValue in funcsDict)
                builder.Add(keyValue.Key, keyValue.Value.ToImmutableArray());
            this.funcs = builder.ToImmutable();

            exactFuncs = exactFuncsBuilder.ToImmutable();
        }
        
        public ImmutableArray<IModuleFuncInfo> Get(M.Name name, int minTypeParamCount)
        {
            return funcs.GetValueOrDefault((name, minTypeParamCount));
        }

        public IModuleFuncInfo? Get(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return exactFuncs.GetValueOrDefault((name, typeParamCount, paramTypes));            
        }
    }
}
