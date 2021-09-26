using Pretune;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    abstract class EnumRuntimeItem : AllocatableRuntimeItem
    {
    }

    partial class Evaluator
    {
        [AutoConstructor]
        partial class IR0EnumRuntimeItem : EnumRuntimeItem
        {
            R.EnumDecl enumDecl;

            public override R.Name Name => new R.Name.Normal(enumDecl.Name);
            public override R.ParamHash ParamHash => new R.ParamHash(enumDecl.TypeParams.Length, default);            

            public override Value Alloc(TypeContext typeContext)
            {
                return new EnumValue(typeContext, null, null);
            }
        }
    }
}