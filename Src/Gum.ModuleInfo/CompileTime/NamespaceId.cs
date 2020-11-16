using System;

namespace Gum.CompileTime
{
    // LocalId
    public struct NamespaceId
    {
        public string Value { get; }
        public NamespaceId(string value) { Value = value; }

        public static implicit operator NamespaceId(string s) => new NamespaceId(s);
    }
}