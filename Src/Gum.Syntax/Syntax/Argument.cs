using Pretune;

namespace Gum.Syntax
{
    public enum ArgumentModifier
    {
        None,
        Params,  // F(params t, params x)
        Ref      // F(ref i)
    }

    // modifier params, ref
    [AutoConstructor]
    public partial class Argument : ISyntaxNode
    {
        public ArgumentModifier ArgumentModifier { get; }
        public Exp Exp { get; }
    }
}