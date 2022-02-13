using Gum.Collections;
using Pretune;

namespace Citron.IR0
{    
    [AutoConstructor, ImplementIEquatable]
    public partial struct ParamHashEntry
    {
        public ParamKind Kind;
        public Path Type;
    }

    [AutoConstructor]
    public partial struct ParamHash
    {
        public static readonly ParamHash None = new ParamHash(0, default);

        public int TypeParamCount { get; }
        public ImmutableArray<ParamHashEntry> Entries { get; }
    }
}