using Gum.Collections;
using Pretune;

namespace Gum.CompileTime
{
    public enum ParamKind
    {
        Normal,
        Params,
        Ref,
    }

    [AutoConstructor, ImplementIEquatable]
    public partial struct Param
    {
        public ParamKind Kind { get; }
        public Type Type { get; }
        public Name Name { get; }
    }
}