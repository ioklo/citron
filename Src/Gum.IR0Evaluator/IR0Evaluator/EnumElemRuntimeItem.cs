using Pretune;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    abstract class EnumElemRuntimeItem : RuntimeItem
    {
    }

    [AutoConstructor]
    partial class IR0EnumElemRuntimeItem : EnumElemRuntimeItem
    {
        public override R.Name Name => enumElem.Name;
        public override R.ParamHash ParamHash => R.ParamHash.None;

        R.EnumElement enumElem;
    }
}