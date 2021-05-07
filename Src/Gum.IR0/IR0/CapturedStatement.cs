using Gum.Collections;
using Pretune;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct CapturedStatement
    {
        public Path? ThisType { get; }
        public ImmutableArray<TypeAndName> OuterLocalVars { get; }
        public Stmt Body { get; }
    }
}