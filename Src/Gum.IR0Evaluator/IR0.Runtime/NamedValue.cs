using System;
using System.Collections.Generic;

namespace Gum.IR0.Runtime
{
    public struct NamedValue
    {
        public string Name;
        public Value Value;

        public NamedValue(string name, Value value)
        {
            Name = name;
            Value = value;
        }

        public override bool Equals(object? obj)
        {
            return obj is NamedValue other &&
                   Name == other.Name &&
                   EqualityComparer<Value>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Value);
        }

        public void Deconstruct(out string name, out Value value)
        {
            name = Name;
            value = Value;
        }

        public static implicit operator (string Name, Value Value)(NamedValue value)
        {
            return (value.Name, value.Value);
        }

        public static implicit operator NamedValue((string Name, Value Value) value)
        {
            return new NamedValue(value.Name, value.Value);
        }
    }
}