using Gum.Collections;
using Pretune;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    abstract class EnumElemRuntimeItem : AllocatableRuntimeItem
    {
        public abstract ImmutableArray<R.EnumElementField> Fields { get; }
    }

    partial class Evaluator
    {
        [AutoConstructor]
        partial class IR0EnumElemRuntimeItem : EnumElemRuntimeItem
        {
            GlobalContext globalContext;

            public override R.Name Name => new R.Name.Normal(enumElem.Name);
            public override R.ParamHash ParamHash => R.ParamHash.None;
            public override ImmutableArray<R.EnumElementField> Fields => enumElem.Fields;

            R.EnumElement enumElem;

            public override Value Alloc(TypeContext typeContext)
            {
                var builder = ImmutableArray.CreateBuilder<Value>(enumElem.Fields.Length);
                foreach (var field in enumElem.Fields)
                {
                    var appliedType = typeContext.Apply(field.Type);
                    var fieldValue = globalContext.AllocValue(appliedType);
                    builder.Add(fieldValue);
                }

                return new EnumElemValue(builder.MoveToImmutable());
            }
        }
    }
}