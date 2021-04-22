using R = Gum.IR0;
using Gum.Infra;
using Pretune;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        abstract class ExpResult
        {
        }

        [AutoConstructor, ImplementIEquatable]
        partial class ExpExpResult : ExpResult
        {
            public R.Exp Exp { get; }
            public TypeValue TypeValue { get; }
        }

        [AutoConstructor, ImplementIEquatable]
        partial class LocExpResult : ExpResult
        {
            public R.Loc Loc { get; }
            public TypeValue TypeValue { get; }
        }
    }
}
