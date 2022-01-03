using Gum.Collections;
using Gum.Infra;
using Pretune;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct OuterLocalVarInfo
    {
        public Path Type { get; }
        public string Name { get; }

        public void Deconstruct(out Path outType, out string outName)
        {
            outType = Type;
            outName = Name;
        }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial struct CapturedStatement
    {
        public Path? ThisType { get; }
        public ImmutableArray<OuterLocalVarInfo> OuterLocalVars { get; }
        public Stmt Body { get; }
    }
}