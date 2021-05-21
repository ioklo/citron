namespace Gum.IR0
{
    public abstract record Argument
    {
        public record Normal(Exp Exp) : Argument;
        public record Params(Exp Exp, int ElemCount) : Argument;
        public record Ref(Loc Loc) : Argument;
    }
}