using Pretune;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    abstract class EnumRuntimeItem : AllocatableRuntimeItem
    {
    }

    [AutoConstructor]
    partial class IR0EnumRuntimeItem : EnumRuntimeItem
    {
        public override R.Name Name => enumDecl.Name;
        public override R.ParamHash ParamHash => new R.ParamHash(enumDecl.TypeParams.Length, default);

        R.EnumDecl enumDecl;

        public override Value Alloc(Evaluator evaluator, TypeContext typeContext)
        {
            return new EnumValue(null, null);
        }
    }
}