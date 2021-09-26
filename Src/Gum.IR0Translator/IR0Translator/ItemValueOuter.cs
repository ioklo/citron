using Gum.Collections;
using System;
using Pretune;

using M = Gum.CompileTime;
using R = Gum.IR0;
using System.Diagnostics;

namespace Gum.IR0Translator
{
    // reserved가 아닌 아이템에서나 존재함
    abstract class ItemValueOuter
    {
        public abstract R.Path.Normal GetRPath();
        public abstract void FillTypeEnv(TypeEnvBuilder builder);
        public abstract ItemValueOuter Apply(TypeEnv typeEnv);
        public abstract TypeEnv GetTypeEnv();
        public abstract int GetTotalTypeParamCount();
    }

    // 최상위
    [AutoConstructor, ImplementIEquatable]
    partial class RootItemValueOuter : ItemValueOuter
    {
        M.ModuleName moduleName;
        M.NamespacePath namespacePath;

        public override R.Path.Normal GetRPath()
        {
            var rmoduleName = RItemFactory.MakeModuleName(moduleName);

            R.Path.Normal path = new R.Path.Root(rmoduleName);
            foreach (var entry in namespacePath.Entries)
                path = new R.Path.Nested(path, new R.Name.Normal(entry.Value), R.ParamHash.None, default);

            return path;
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
            return new TypeEnv(default);
        }

        public override int GetTotalTypeParamCount()
        {
            return 0;
        }
    }

    [AutoConstructor, ImplementIEquatable]
    partial class NestedItemValueOuter : ItemValueOuter
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
