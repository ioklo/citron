using Gum.Infra;
using Gum.Collections;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    // Error/NotFound/Value
    abstract record ItemResult
    {
        public record Error : ItemResult
        {
            public record VarWithTypeArg : Error
            {
                public static readonly VarWithTypeArg Instance = new VarWithTypeArg();
                VarWithTypeArg() { }
            }

            public record MultipleCandidates : Error
            {
                public static readonly MultipleCandidates Instance = new MultipleCandidates();
                MultipleCandidates() { }
            }
        }

        public record NotFound : ItemResult
        {
            public static readonly NotFound Instance = new NotFound();
            NotFound() { }
        }

        public abstract record Valid : ItemResult;
        
        public record Type(TypeValue TypeValue) : Valid;
        public record Funcs(ImmutableArray<FuncValue> FuncValues) : Valid;
        public record MemberVar(MemberVarValue MemberVarValue) : Valid;        
    }
}