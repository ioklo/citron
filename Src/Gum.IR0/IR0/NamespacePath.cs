using Pretune;
using System.Collections.Immutable;

namespace Gum.IR0
{
    [ImplementIEquatable]
    public partial struct NamespacePath
    {
        public static readonly NamespacePath Root = new NamespacePath(ImmutableArray<NamespaceName>.Empty);

        public ImmutableArray<NamespaceName> Entries { get; }
        public bool IsRoot { get => Entries.IsEmpty; }

        public NamespacePath(ImmutableArray<NamespaceName> entries)
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