using Pretune;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    abstract class EnumElemFieldRuntimeItem : RuntimeItem
    {
        public abstract Value GetMemberValue(EnumElemValue instance);
    }

    partial class Evaluator
    {
        [AutoConstructor]
        partial class IR0EnumElemFieldRuntimeItem : EnumElemFieldRuntimeItem
        {
            public override R.Name Name { get; }
            public override R.ParamHash ParamHash => R.ParamHash.None;
            int index;

            public override Value GetMemberValue(EnumElemValue instance)
            {
                return instance.Fields[index];
            }
        }
    }
}