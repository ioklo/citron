using Pretune;
using Gum.Collections;

namespace Gum.IR0
{
    class TypeContextBuilder
    {
        ImmutableArray<TypeContext.Entry>.Builder builder;

        public TypeContextBuilder()
        {
            builder = ImmutableArray.CreateBuilder<TypeContext.Entry>();
        }

        public TypeContextBuilder Add(int depth, int index, Type type)
        {
            var entry = new TypeContext.Entry(depth, index, type);
            builder.Add(entry);
            return this;
        }

        public TypeContext Build()
        {
            return new TypeContext(builder.ToImmutable());
        }
    }

    // 타입 인자 리스트
    [ImplementIEquatable]
    public partial class TypeContext
    {
        [AutoConstructor, ImplementIEquatable]
        internal partial struct Entry
        {
            public int Depth { get; }
            public int Index { get; }
            public Type Type { get; }
        }

        public static readonly TypeContext Empty;
        static TypeContext()
        {
            Empty = new TypeContext(ImmutableArray<Entry>.Empty);
        }

        // T<int, X<int>.Y<short.string>>.U<bool>
        // => [(0, 0, int), (0, 1, X<int>.Y<short, string>), (1, 0, bool)]
        // => [(0, 0, int, []), (0, 1, X.Y, [(0, 0, int), (1, 0, short), (1, 1, string)]), (1, 0, bool, [])]
        ImmutableArray<Entry> entries;

        internal TypeContext(ImmutableArray<Entry> entries)
        {
            this.entries = entries;
        }
    }
}