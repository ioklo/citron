using System;
using System.Collections.Generic;

namespace Gum.IR0
{
    // int a
    public struct TypeAndName
    {
        public TypeId Type { get; }
        public string Name { get; }

        // out int& a
        public TypeAndName(TypeId type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}