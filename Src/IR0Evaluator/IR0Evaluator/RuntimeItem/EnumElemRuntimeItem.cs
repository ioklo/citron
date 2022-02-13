using Citron.Collections;
using Citron.CompileTime;
using Pretune;
using System;
using R = Citron.IR0;

namespace Citron.IR0Evaluator
{
    abstract class EnumElemRuntimeItem : AllocatableRuntimeItem
    {
        public abstract ImmutableArray<R.EnumElementField> Fields { get; }
    }

        [AutoConstructor]
        partial class IR0EnumElemRuntimeItem : EnumElemRuntimeItem
        {
            IR0GlobalContext globalContext;

            public override R.Name Name => new R.Name.Normal(enumElem.Name);
            public override R.ParamHash ParamHash => R.ParamHash.None;
            public override ImmutableArray<R.EnumElementField> Fields => enumElem.Fields;

            R.EnumElement enumElem;

            //public override Value Alloc(TypeContext typeContext)
            //{
            //    var builder = ImmutableArray.CreateBuilder<Value>(enumElem.Fields.Length);
            //    foreach (var field in enumElem.Fields)
            //    {
            //        var appliedType = typeContext.Apply(field.Type);
            //        var fieldValue = globalContext.AllocValue(appliedType);
            //        builder.Add(fieldValue);
            //    }

            //    return new EnumElemValue(builder.MoveToImmutable());
            //}
        }
 
}