using Gum.Collections;
using Pretune;

namespace Gum.CompileTime
{
    [AutoConstructor]
    public partial struct ParamInfo
    {
        public int VariadicParamIndex { get; }
        public ImmutableArray<(Type Type, Name Name)> Parameters { get; }
    }
}