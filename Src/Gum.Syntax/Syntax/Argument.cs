using Pretune;

namespace Gum.Syntax
{
    // modifier params, ref
    public abstract record Argument : ISyntaxNode
    {
        public record Normal(Exp Exp) : Argument;
        public record Params(Exp Exp) : Argument;
        public record Ref(Exp Exp) : Argument;
    }
}