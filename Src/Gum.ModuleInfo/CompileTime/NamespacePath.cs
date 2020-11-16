using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Gum.CompileTime
{
    public struct NamespacePath
    {
        public ImmutableArray<NamespaceId> Entries { get; }

        public static NamespacePath Root { get; } = new NamespacePath(ImmutableArray<NamespaceId>.Empty);

        private NamespacePath(ImmutableArray<NamespaceId> entries)
        {
            Entries = entries;
        }

        public NamespacePath(NamespaceId hdEntry, params NamespaceId[] tlEntries)
        {
            var builder = ImmutableArray.CreateBuilder<NamespaceId>(tlEntries.Length + 1);
            builder.Add(hdEntry);
            builder.AddRange(tlEntries);
            Entries = builder.MoveToImmutable();
        }

        public NamespacePath(IEnumerable<NamespaceId> entries)
        {
            Entries = entries.ToImmutableArray();
        }
    }
}