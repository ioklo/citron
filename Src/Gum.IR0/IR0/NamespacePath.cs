using Pretune;
using System.Collections.Immutable;

namespace Gum.IR0
{
    [ImplementIEquatable]
    public partial struct NamespacePath
    {
        public static NamespacePath Root { get; } = new NamespacePath(ImmutableArray<NamespaceName>.Empty);

        public ImmutableArray<NamespaceName> Entries { get; }
        public bool IsRoot { get => Entries.IsEmpty; }

        internal NamespacePath(ImmutableArray<NamespaceName> entries)
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