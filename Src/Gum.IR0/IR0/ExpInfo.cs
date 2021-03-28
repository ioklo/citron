using Pretune;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct ExpInfo
    {
        public Exp Exp { get; }
        public Type Type { get; }
    }
}
