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

        // anonymous type names
        public record Anonymous(AnonymousId Id) : Name;
    }
}