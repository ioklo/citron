using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gum.Syntax
{   
    // 가장 외곽
    public partial class Script : ISyntaxNode
    {
        public ImmutableArray<Element> Elements { get; }
        public Script(IEnumerable<Element> elements)
        {
            Elements = elements.ToImmutableArray();
        }

        public Script(params Element[] elements)
        {
            Elements = elements.ToImmutableArray();
        }

        public override bool Equals(object? obj)
        {
            return obj is Script script && Enumerable.SequenceEqual(Elements, script.Elements);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            foreach (var elem in Elements)
                hashCode.Add(elem);

            return hashCode.ToHashCode();
        }

        public static bool operator ==(Script? left, Script? right)
        {
            return EqualityComparer<Script?>.Default.Equals(left, right);
        }

        public static bool operator !=(Script? left, Script? right)
        {
            return !(left == right);
        }
    }
}