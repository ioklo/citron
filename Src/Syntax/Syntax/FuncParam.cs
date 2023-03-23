using Pretune;
using System;
using System.Collections.Generic;

namespace Citron.Syntax
{
    public enum FuncParamKind
    {
        Normal,
        Params,
    }

    // int a, out int&a
    [AutoConstructor, ImplementIEquatable]
    public partial struct FuncParam
    { 
        public FuncParamKind Kind { get; }
        public TypeExp Type { get; }
        public string Name { get; }
    }
}