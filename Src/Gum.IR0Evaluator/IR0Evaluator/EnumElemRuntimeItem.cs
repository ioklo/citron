using Gum.Collections;
using Pretune;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    abstract class EnumElemRuntimeItem : RuntimeItem
    {
        public abstract ImmutableArray<R.TypeAndName> Params { get; }
    }

    [AutoConstructor]
    partial class IR0EnumElemRuntimeItem : EnumElemRuntimeItem
    {
        public override R.Name Name => enumElem.Name;
        public override R.ParamHash ParamHash => R.ParamHash.None;
        public override ImmutableArray<R.TypeAndName> Params => enumElem.Params;

        R.EnumElement enumElem;
    }
}