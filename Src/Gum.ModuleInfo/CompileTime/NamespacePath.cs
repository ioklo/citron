using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Linq;

namespace Gum.CompileTime
{
    public struct NamespacePath : IEquatable<NamespacePath>
    {
        public ImmutableArray<NamespaceName> Entries { get; }

        public static NamespacePath Root { get; } = new NamespacePath(ImmutableArray<NamespaceName>.Empty);

        public bool IsRoot { get => Entries.IsEmpty; }

        private NamespacePath(ImmutableArray<NamespaceName> entries)
        {
            Entries = entries;
        }

        public NamespacePath(NamespaceName hdEntry, params NamespaceName[] tlEntries)
        {
            var builder = ImmutableArray.CreateBuilder<NamespaceName>(tlEntries.Length + 1);
            builder.Add(hdEntry);
            builder.AddRange(tlEntries);
            Entries = builder.MoveToImmutable();
        }
        
        public override bool Equals(object? obj)
        {
            return obj is NamespacePath path && Equals(path);
        }

        public bool Equals(NamespacePath other)
        {
            return Entries.Equals(other.Entries) &&
                   IsRoot == other.IsRoot;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Entries, IsRoot);
        }
    }
}