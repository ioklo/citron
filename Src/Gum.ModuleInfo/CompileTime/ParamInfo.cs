using Gum.Collections;
using Pretune;

namespace Gum.CompileTime
{
    public enum ParamKind
    {
        Default,
        Params,
        Ref,
    }

    [AutoConstructor, ImplementIEquatable]
    public partial struct Param
    {
        public ParamKind Kind { get; }
        public TypeId Type { get; }
        public Name Name { get; }
    }
}