using Pretune;
using System;
using System.Collections.Generic;

namespace Gum.IR0
{
    // int a
    [AutoConstructor, ImplementIEquatable]
    public partial struct TypeAndName
    {
        public Path Type { get; }
        public string Name { get; }

        public void Deconstruct(out Path outType, out string outName)
        {
            outType = Type;
            outName = Name;
        }
    }
}