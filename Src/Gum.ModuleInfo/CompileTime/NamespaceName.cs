using Pretune;
using System;

namespace Gum.CompileTime
{
    // LocalId
    [ImplementIEquatable]
    public partial struct NamespaceName
    {
        public string Value { get; }
        public NamespaceName(string value) { Value = value; }

        public static implicit operator NamespaceName(string s) => new NamespaceName(s);
    }
}