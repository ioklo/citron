using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;

using Citron.Module;
using System.Runtime.CompilerServices;

namespace Citron.Symbol
{    
    // decl type env
    // 코드 decl/body에 나타나는 TypeEnv만을 가리킨다 그건 TypeVar만 있을 것이다
    // 실행중에는 runtime type env
    public struct TypeEnv
    {
        ImmutableArray<IType> data;
        public static TypeEnv Empty = new TypeEnv(default);
        
        internal TypeEnv(ImmutableArray<IType> data)
        {
            this.data = data;
        }

        public IType GetValue(int index)
        {
            return data[index];
        }

        public TypeEnv AddTypeArgs(ImmutableArray<IType> typeArgs)
        {
            return new TypeEnv(data.AddRange(typeArgs));
        }
    }
}