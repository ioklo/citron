using Pretune;
using System;
using System.Collections.Generic;

namespace Gum.Syntax
{
    // int a, out int&a
    [AutoConstructor, ImplementIEquatable]
    public partial struct TypeAndName
    {
        public TypeExp Type { get; }
        public string Name { get; }
    }
}