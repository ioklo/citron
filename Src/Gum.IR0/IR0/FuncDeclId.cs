using Pretune;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct FuncDeclId
    {
        public int Value { get; }
    }
}