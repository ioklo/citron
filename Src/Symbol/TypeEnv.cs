using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;

using Citron.Module;

namespace Citron.Symbol
{    
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