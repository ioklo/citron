using Citron.Collections;
using Pretune;

namespace Citron.CompileTime
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
        public SymbolId Type { get; }
        public Name Name { get; }
    }
}