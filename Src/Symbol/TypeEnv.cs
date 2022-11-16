using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;

using Citron.Module;

namespace Citron.Symbol
{    
    // decl type env
    // 코드 decl/body에 나타나는 TypeEnv만을 가리킨다 그건 TypeVar만 있을 것이다
    // 실행중에는 runtime type env
    public struct TypeEnv
    {
        ImmutableArray<ITypeSymbol> data;
        public static TypeEnv Empty = new TypeEnv(default);
        
        internal TypeEnv(ImmutableArray<ITypeSymbol> data)
        {
            this.data = data;
        }

        public ITypeSymbol GetValue(int index)
        {
            return data[index];
        }

        public TypeEnv AddTypeArgs(ImmutableArray<ITypeSymbol> typeArgs)
        {
            return new TypeEnv(data.AddRange(typeArgs));
        }
    }
}