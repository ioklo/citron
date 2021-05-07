using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Linq;
using Pretune;

namespace Gum.CompileTime
{
    [ImplementIEquatable]
    public partial struct NamespacePath
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
        
    }
}