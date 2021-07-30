using Gum.Infra;
using Gum.Collections;

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
        
        // ItemValue류 대신에 => 아직 정보가 부족해서 그랬다
        public record Type(ItemValueOuter Outer, IModuleTypeInfo TypeInfo) : Valid;
        public record Constructors(NormalTypeValue Outer, ImmutableArray<IModuleConstructorInfo> ConstructorInfos) : Valid;
        public record Funcs(ItemValueOuter Outer, ImmutableArray<IModuleFuncInfo> FuncInfos, bool IsInstanceFunc) : Valid;
        public record MemberVar(NormalTypeValue Outer, IModuleMemberVarInfo MemberVarInfo) : Valid;
        public record EnumElem(EnumTypeValue Outer, IModuleEnumElemInfo EnumElemInfo) : Valid;
    }
}