using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Gum.CompileTime
{
    public struct AppliedItemPath
    {
        public NamespacePath NamespacePath { get; }    // root namespace
        public ImmutableArray<AppliedItemPathEntry> OuterEntries { get; }
        public AppliedItemPathEntry Entry { get; }

        public AppliedItemPath(NamespacePath namespacePath, AppliedItemPathEntry entry)
        {
            NamespacePath = namespacePath;
            OuterEntries = ImmutableArray<AppliedItemPathEntry>.Empty;
            Entry = entry;
        }

        public AppliedItemPath(NamespacePath namespacePath, IEnumerable<AppliedItemPathEntry> outerEntries, AppliedItemPathEntry entry)
        {
            NamespacePath = namespacePath;
            OuterEntries = outerEntries.ToImmutableArray();
            Entry = entry;
        }

        public ItemPath GetItemPath()
        {
            return new ItemPath(NamespacePath, OuterEntries.Select(entry => entry.GetItemPathEntry()), Entry.GetItemPathEntry());
        }
    }
}
