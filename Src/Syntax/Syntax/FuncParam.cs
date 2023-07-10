using Pretune;
using System;
using System.Collections.Generic;

namespace Citron.Syntax
{
    // int a, out int&a
    [AutoConstructor, ImplementIEquatable]
    public partial struct FuncParam
    {   
        public TypeExp Type { get; }
        public string Name { get; }
    }
}