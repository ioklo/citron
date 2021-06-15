using Gum.Collections;
using Gum.Infra;
using Pretune;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct OuterLocalVarInfo : IPure
    {
        public Path Type { get; }
        public Name Name { get; }

        public void Deconstruct(out Path outType, out Name outName)
        {
            outType = Type;
            outName = Name;
        }

        public void EnsurePure() { }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial struct CapturedStatement : IPure
    {
        public Path? ThisType { get; }
        public ImmutableArray<OuterLocalVarInfo> OuterLocalVars { get; }
        public Stmt Body { get; }

        public void EnsurePure()
        {
            Misc.EnsurePure(ThisType!);
            Misc.EnsurePure(OuterLocalVars);
            Misc.EnsurePure(Body);
        }
    }
}