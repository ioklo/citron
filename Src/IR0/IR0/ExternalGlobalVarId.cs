using Pretune;

namespace Citron.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct ExternalGlobalVarId
    {
        public int Value { get; }
    }
}