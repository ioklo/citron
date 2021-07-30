using Gum.Infra;
using Pretune;
using Gum.Collections;

using M = Gum.CompileTime;
using R = Gum.IR0;


namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        abstract record ExpResult
        {
            public record Namespace : ExpResult;
            public record Type(TypeValue TypeValue) : ExpResult;
            public record Funcs(ItemValueOuter Outer, ImmutableArray<IModuleFuncInfo> FuncInfos, ImmutableArray<TypeValue> TypeArgs, R.Loc? Instance) : ExpResult;
            public record EnumElem(EnumElemTypeValue EnumElemTypeValue) : ExpResult;
            public record Exp(R.Exp Result, TypeValue TypeValue) : ExpResult;
            public record Loc(R.Loc Result, TypeValue TypeValue) : ExpResult;
        }
    }
}
