namespace Citron.IR0
{
    // 함수 아규먼트
    public abstract record Argument
    {
        public record Normal(Exp Exp) : Argument;
        public record Params(Exp Exp, int ElemCount) : Argument; // tuple 타입
    }
}
