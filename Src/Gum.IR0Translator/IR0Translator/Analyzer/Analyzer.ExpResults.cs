using R = Gum.IR0;
using Gum.Infra;
using Pretune;
using Gum.Collections;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        abstract class ExpResult
        {
        }

        [AutoConstructor, ImplementIEquatable]
        partial class NamespaceExpResult : ExpResult
        {
        }

        [AutoConstructor, ImplementIEquatable]
        partial class TypeExpResult : ExpResult
        {            
            public TypeValue TypeValue { get; }
        }

        // 함수 덩어리
        [AutoConstructor, ImplementIEquatable]
        partial class FuncsExpResult : ExpResult
        {
            public ImmutableArray<FuncValue> FuncValues { get; }
        }

        // enum constructor를 의미하는
        [AutoConstructor, ImplementIEquatable]
        partial class EnumElemExpResult : ExpResult
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
