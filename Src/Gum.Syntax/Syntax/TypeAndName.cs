using System;
using System.Collections.Generic;

namespace Gum.Syntax
{
    // int a
    public struct TypeAndName
    {
        public TypeExp Type { get; }
        public string Name { get; }

        // out int& a
        public TypeAndName(TypeExp type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}