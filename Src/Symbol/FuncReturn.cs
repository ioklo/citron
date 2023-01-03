﻿using Pretune;

namespace Citron.Symbol
{   
    [AutoConstructor]
    public partial struct FuncReturn
    {
        public bool IsRef { get; }
        public IType Type { get; }

        public FuncReturn Apply(TypeEnv typeEnv)
        {
            var appliedType = Type.Apply(typeEnv);
            return new FuncReturn(IsRef, appliedType);
        }
    }
}