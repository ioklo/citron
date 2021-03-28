using Pretune;
using System;
using System.Collections.Generic;

namespace Gum.IR0
{
    // int a
    [AutoConstructor, ImplementIEquatable]
    public partial struct TypeAndName
    {
        public Type Type { get; }
        public string Name { get; }
    }
}