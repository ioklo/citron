using Pretune;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct ExternalGlobalVarId
    {
        public int Value { get; }
    }
}