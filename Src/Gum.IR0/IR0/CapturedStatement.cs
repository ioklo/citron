using Gum.Collections;
using Pretune;

namespace Gum.IR0
{
    [AutoConstructor]
    public partial struct CapturedStatement
    {
        public Type? ThisType { get; }
        public ImmutableArray<TypeAndName> OuterLocalVars { get; }
        public Stmt Body { get; }
    }
}