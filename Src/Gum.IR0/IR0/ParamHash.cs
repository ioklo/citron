using Gum.Collections;
using Pretune;

namespace Gum.IR0
{
    [AutoConstructor]
    public partial struct ParamHash
    {
        public static readonly ParamHash None = new ParamHash(0, default);

        public int TypeParamCount { get; }
        public ImmutableArray<Path> FuncParamTypes { get; }
    }
}