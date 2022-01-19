using Gum.Collections;
using System;
using Pretune;

using M = Gum.CompileTime;
using System.Diagnostics;

namespace Gum.Analysis
{
    // reserved가 아닌 아이템에서나 존재함
    public abstract class ItemValueOuter
    {
        public abstract R.Path.Normal GetRPath();
        public abstract void FillTypeEnv(TypeEnvBuilder builder);
        public abstract ItemValueOuter Apply(TypeEnv typeEnv);
        public abstract TypeEnv GetTypeEnv();
        public abstract int GetTotalTypeParamCount();
    }

    // IModuleItemInfo류를 상위로 (TypeArgs가 없는 것들)
    [AutoConstructor, ImplementIEquatable]
    public partial class RootItemValueOuter : ItemValueOuter
    {
        IModuleItemDecl item;

        public override R.Path.Normal GetRPath()
        {
            var entry = item.GetEntry();
            Debug.Assert(entry.TypeParamCount == 0 && entry.ParamTypes.IsEmpty);

            var rmoduleName = RItemFactory.MakeModuleName(entry.Name);
            return new R.Path.Root(rmoduleName);
        }

        public override void FillTypeEnv(TypeEnvBuilder builder)
        {
        }

        public override ItemValueOuter Apply(TypeEnv typeEnv)
        {
            return this;
        }

        public override TypeEnv GetTypeEnv()
        {
            return TypeEnv.None;
        }

        public override int GetTotalTypeParamCount()
        {
            return 0;
        }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class NestedItemValueOuter : ItemValueOuter
    {
        public ItemValue ItemValue { get; }

        public override R.Path.Normal GetRPath()
        {
            var path = ItemValue.GetRPath() as R.Path.Normal;
            Debug.Assert(path != null);

            return path;
        }

        public override void FillTypeEnv(TypeEnvBuilder builder)
        {
            ItemValue.FillTypeEnv(builder);
        }

        public override ItemValueOuter Apply(TypeEnv typeEnv)
        {
            var appliedOuter = ItemValue.Apply_ItemValue(typeEnv);
            return new NestedItemValueOuter(appliedOuter);
        }

        public override TypeEnv GetTypeEnv()
        {
            return ItemValue.MakeTypeEnv();
        }

        public override int GetTotalTypeParamCount()
        {
            return ItemValue.GetTotalTypeParamCount();
        }
    }
}
