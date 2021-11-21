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
        public ImmutableArray<Name> Entries { get; }

        public static NamespacePath Root { get; } = new NamespacePath(ImmutableArray<Name>.Empty);

        public bool IsRoot { get => Entries.IsEmpty; }

        private NamespacePath(ImmutableArray<Name> entries)
        {
            Entries = entries;
        }

        public NamespacePath(Name hdEntry, params Name[] tlEntries)
        {
            var builder = ImmutableArray.CreateBuilder<Name>(tlEntries.Length + 1);
            builder.Add(hdEntry);
            builder.AddRange(tlEntries);
            Entries = builder.MoveToImmutable();
        }       
        
    }
}