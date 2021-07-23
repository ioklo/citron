using Gum.Infra;
using System.Diagnostics;

namespace Gum.IR0
{   
    public abstract record Name : IPure
    {
        public void EnsurePure() { }

        public static implicit operator Name(string x) => new Normal(x);

        [DebuggerDisplay("{Value}")]
        public record Normal(string Value) : Name;
        public record IndexerGet : Name;
        public record IndexerSet : Name;        
        public record OpInc : Name;
        public record OpDec : Name;

        // anonymous type names
        [DebuggerDisplay("#Anonymous_{Id}")]
        public record Anonymous(AnonymousId Id) : Name;

        public record Constructor : Name
        {
            public static readonly Constructor Instance = new Constructor();
            Constructor() { }
        }
    }
}