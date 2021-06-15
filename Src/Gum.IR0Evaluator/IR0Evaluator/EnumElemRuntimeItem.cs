using Gum.Collections;
using Pretune;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    abstract class EnumElemRuntimeItem : AllocatableRuntimeItem
    {
        public abstract ImmutableArray<R.Param> Params { get; }
    }

    partial class Evaluator
    {
        [AutoConstructor]
        partial class IR0EnumElemRuntimeItem : EnumElemRuntimeItem
        {
            public override R.Name Name => enumElem.Name;
            public override R.ParamHash ParamHash => R.ParamHash.None;
            public override ImmutableArray<R.Param> Params => enumElem.Params;

            R.EnumElement enumElem;

            public override Value Alloc(Evaluator evaluator, TypeContext typeContext)
            {
                var builder = ImmutableArray.CreateBuilder<Value>(enumElem.Params.Length);
                foreach (var field in enumElem.Params)
                {
                    var appliedType = typeContext.Apply(field.Type);
                    var fieldValue = evaluator.AllocValue(appliedType);
                    builder.Add(fieldValue);
                }

                return new EnumElemValue(builder.MoveToImmutable());
            }
        }
    }
}