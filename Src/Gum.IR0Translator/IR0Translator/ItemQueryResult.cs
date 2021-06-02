using Gum.Infra;
using Gum.Collections;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    // Error/NotFound/Value
    abstract record ItemQueryResult
    {
        public record Error : ItemQueryResult
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

        public record NotFound : ItemQueryResult
        {
            public static readonly NotFound Instance = new NotFound();
            NotFound() { }
        }

        public abstract record Valid : ItemQueryResult;
        
        // ItemValue류 대신에, 
        public record Type(ItemValueOuter Outer, M.TypeInfo TypeInfo) : Valid;
        public record Funcs(ItemValueOuter Outer, ImmutableArray<M.FuncInfo> FuncInfos, bool IsInstanceFunc) : Valid;
        public record MemberVar(NormalTypeValue Outer, M.MemberVarInfo MemberVarInfo) : Valid;
        public record EnumElem(EnumTypeValue Outer, M.EnumElemInfo EnumElemInfo) : Valid;
    }
}