using Citron.Collections;
using System;
using Citron.Symbol;
using Citron.Syntax;
using Citron.Module;

namespace Citron.Analysis
{
    public partial class TypeExpEvaluator
    {
        class LocalContext
        {
            DeclSymbolPath? declPath; // 현재 decl 위치
            ImmutableDictionary<(Name Name, int TypeParamCount), Func<TypeExp, TypeExpInfo>> dict;

            public LocalContext()
            {
                declPath = null;
                dict = ImmutableDictionary<(Name Name, int TypeParamCount), Func<TypeExp, TypeExpInfo>>.Empty;
            }

            LocalContext(DeclSymbolPath? declPath, ImmutableDictionary<(Name Name, int TypeParamCount), Func<TypeExp, TypeExpInfo>> dict)
            {
                this.declPath = declPath;
                this.dict = dict;
            }

            public LocalContext NewLocalContext(Name.Normal name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
            {
                var newDeclPath = declPath.Child(name, typeParamCount, paramIds);
                return new LocalContext(newDeclPath, dict);
            }
            
            public LocalContext NewLocalContext(ImmutableArray<string> typeParams)
            {
                var newDict = dict.Add(typeParams);
                return new LocalContext(declPath, newDict);
            }

            public TypeExpInfo? TryMakeTypeVar(Name name, TypeExp typeExp)
            {
                if (dict.TryGetValue(name, out var constructor))
                    return constructor.Invoke(typeExp);

                return null;
            }

            

            public TypeExpInfo? MakeTypeExpInfo(string name, int typeParamCount, TypeExp typeExp)
            {
                var key = (new Name.Normal(name), typeParamCount);

                if (dict.TryGetValue(key, out var constructor))
                    return constructor.Invoke(typeExp);

                return null;
            }
        }
    }
}
