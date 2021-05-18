using Gum.Collections;
using Gum.Infra;
using Pretune;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct CapturedStatement : IPure
    {
        public Path? ThisType { get; }
        public ImmutableArray<TypeAndName> OuterLocalVars { get; }
        public Stmt Body { get; }

        public void EnsurePure()
        {
            Misc.EnsurePure(ThisType);
            Misc.EnsurePure(OuterLocalVars);
            Misc.EnsurePure(Body);
        }
    }
}