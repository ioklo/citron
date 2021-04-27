namespace Gum.IR0
{   
    public abstract record Name
    {
        public static implicit operator Name(string x) => new Normal(x);

        public record Normal(string Value) : Name;
        public record IndexerGet : Name;
        public record IndexerSet : Name;        
        public record OpInc : Name;
        public record OpDec : Name;

        // Reserved Type name        
        public record TypeVar(int Depth, int Index) : Name;
        public record Void : Name
        {
            public static readonly Name Instance = new Void();
            Void() { }
        }

        // for lambda
        public record AnonymousLambda(LambdaId lambdaId) : Name;
    }
}