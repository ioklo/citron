﻿using Gum.Collections;
using System;
using Pretune;

using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    // reserved가 아닌 아이템에서나 존재함
    abstract class ItemValueOuter
    {
        public abstract R.Path MakeRPath(R.Name name, R.ParamHash paramHash, ImmutableArray<R.Path> typeArgs);
        public abstract int FillTypeEnv(TypeEnvBuilder builder);
    }

    // 최상위
    [AutoConstructor]
    partial class RootItemValueOuter : ItemValueOuter
    {
        M.ModuleName moduleName;
        M.NamespacePath namespacePath;

        public override R.Path MakeRPath(R.Name name, R.ParamHash paramHash, ImmutableArray<R.Path> typeArgs)
        {
            var rmoduleName = RItemFactory.MakeModuleName(moduleName);

            R.Path.Normal path = new R.Path.Root(rmoduleName);
            foreach (var entry in namespacePath.Entries)
                path = new R.Path.Nested(path, entry.Value, R.ParamHash.None, default);

            return new R.Path.Nested(path, name, paramHash, typeArgs);
        }

        public override int FillTypeEnv(TypeEnvBuilder builder)
        {
            return 0;
        }
    }

    [AutoConstructor]
    partial class NestedItemValueOuter : ItemValueOuter
    {
        ItemValue outer;

        public override R.Path MakeRPath(R.Name name, R.ParamHash paramHash, ImmutableArray<R.Path> typeArgs)
        {
            var router = outer.GetRType() as R.Path.Normal;
            Debug.Assert(router != null);

            return new R.Path.Nested(router, name, paramHash, typeArgs);
        }

        public override int FillTypeEnv(TypeEnvBuilder builder)
        {
            return outer.FillTypeEnv(builder);
        }
    }
}
