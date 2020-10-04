using System;
using System.Collections.Generic;

namespace Gum.IR0
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

        public override bool Equals(object? obj)
        {
            return obj is TypeAndName param &&
                   EqualityComparer<TypeExp>.Default.Equals(Type, param.Type) &&
                   Name == param.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Name);
        }

        public static bool operator ==(TypeAndName left, TypeAndName right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TypeAndName left, TypeAndName right)
        {
            return !(left == right);
        }
    }
}