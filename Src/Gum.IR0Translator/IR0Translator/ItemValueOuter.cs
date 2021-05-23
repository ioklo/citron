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
        public abstract R.Path.Nested GetRPath(R.Name name, R.ParamHash paramHash, ImmutableArray<R.Path> typeArgs);
        public abstract void FillTypeEnv(TypeEnvBuilder builder);
        public abstract ItemValueOuter Apply(TypeEnv typeEnv);
        public abstract TypeEnv GetTypeEnv();
    }

    // 최상위
    [AutoConstructor, ImplementIEquatable]
    partial class RootItemValueOuter : ItemValueOuter
    {
        M.ModuleName moduleName;
        M.NamespacePath namespacePath;

        public override R.Path.Nested GetRPath(R.Name name, R.ParamHash paramHash, ImmutableArray<R.Path> typeArgs)
        {
            var rmoduleName = RItemFactory.MakeModuleName(moduleName);

            R.Path.Normal path = new R.Path.Root(rmoduleName);
            foreach (var entry in namespacePath.Entries)
                path = new R.Path.Nested(path, entry.Value, R.ParamHash.None, default);

            return new R.Path.Nested(path, name, paramHash, typeArgs);
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
    }

    [AutoConstructor, ImplementIEquatable]
    partial class NestedItemValueOuter : ItemValueOuter
    {
        public ItemValue ItemValue { get; }

        public override R.Path.Nested GetRPath(R.Name name, R.ParamHash paramHash, ImmutableArray<R.Path> typeArgs)
        {
            var router = ItemValue.GetRPath() as R.Path.Normal;
            Debug.Assert(router != null);

            return new R.Path.Nested(router, name, paramHash, typeArgs);
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
    }
}
