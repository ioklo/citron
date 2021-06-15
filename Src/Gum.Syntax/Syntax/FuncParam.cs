using Pretune;
using System;
using System.Collections.Generic;

namespace Gum.Syntax
{
    public enum FuncParamKind
    {
        Normal,
        Params,
        Ref,
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