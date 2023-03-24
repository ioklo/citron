using Pretune;

namespace Citron.Syntax
{
    // modifier params, ref
    public abstract record class Argument : ISyntaxNode
    {
        public record class Normal(bool IsRef, Exp Exp) : Argument;
        public record class Params(Exp Exp) : Argument;
    }
}