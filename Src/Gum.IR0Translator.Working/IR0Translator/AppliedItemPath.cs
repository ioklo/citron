using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Gum.IR0
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

        public AppliedItemPath Append(AppliedItemPathEntry entry)
        {
            return new AppliedItemPath(NamespacePath, OuterEntries.Append(Entry), entry);
        }

        public AppliedItemPath? GetOuter()
        {
            if (OuterEntries.Length == 0)
                return null;

            return new AppliedItemPath(NamespacePath, OuterEntries.RemoveAt(OuterEntries.Length - 1), OuterEntries[OuterEntries.Length - 1]);
        }
    }
}
