using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;

using M = Gum.CompileTime;

namespace Gum.Analysis
{    
    public struct TypeEnv
    {
        ImmutableArray<ITypeSymbolNode> data;
        public static TypeEnv Empty = new TypeEnv(default);
        
        internal TypeEnv(ImmutableArray<ITypeSymbolNode> data)
        {
            this.data = data;
        }

        public ITypeSymbolNode GetValue(int index)
        {
            return data[index];
        }

        public TypeEnv AddTypeArgs(ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            return new TypeEnv(data.AddRange(typeArgs));
        }
    }
}