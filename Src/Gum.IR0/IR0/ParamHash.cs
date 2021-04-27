using Pretune;

namespace Gum.IR0
{
    [AutoConstructor]
    public partial struct ParamHash
    {
        public static readonly ParamHash None = new ParamHash(string.Empty);

        public string Value { get; }
    }
}