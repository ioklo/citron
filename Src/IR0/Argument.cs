namespace Citron.IR0
{
    public abstract record class Argument
    {
        public record class Normal(Exp Exp) : Argument;
        public record class Params(Exp Exp, int ElemCount) : Argument;
        public record class Ref(Loc Loc) : Argument;
    }
}