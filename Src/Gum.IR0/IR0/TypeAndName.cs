using System;
using System.Collections.Generic;

namespace Gum.IR0
{
    // int a
    public struct TypeAndName
    {
        public Type Type { get; }
        public string Name { get; }

        // out int& a
        public TypeAndName(Type type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}