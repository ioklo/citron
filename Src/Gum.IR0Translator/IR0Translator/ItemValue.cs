using Pretune;
using R = Gum.IR0;
using M = Gum.CompileTime;
using Gum.Collections;
using System.Diagnostics;

namespace Gum.IR0Translator
{
    // TypeValue, FuncValue, MemberVarValue
    abstract class ItemValue
    {
        internal virtual int FillTypeEnv(TypeEnvBuilder builder) { return 0; }

        protected TypeEnv MakeTypeEnv()
        {
            // TypeContext 빌더랑 똑같이 생긴
            var builder = new TypeEnvBuilder();
            FillTypeEnv(builder);
            return builder.Build();
        }

        public abstract R.Path GetRType();
    }
    
    // reserved가 아닌 아이템에서나 존재함
    abstract class ItemValueOuter
    {
        public abstract R.Path MakeRPath(R.Name name, R.ParamHash paramHash, ImmutableArray<R.Path> typeArgs);
    }
    
    // 최상위
    [AutoConstructor]
    class RootItemValueOuter : ItemValueOuter
    {
        M.ModuleName moduleName;
        M.NamespacePath namespacePath;

        public override R.Path MakeRPath(R.Name name, R.ParamHash paramHash, ImmutableArray<R.Path> typeArgs)
        {
            var rmoduleName = RItemFactory.MakeModuleName(moduleName);
            var rnamespacePath = RItemFactory.MakeNamespacePath(namespacePath);

            return new R.Path.Root(rmoduleName, rnamespacePath, name, paramHash, typeArgs);
        }
    }

    [AutoConstructor]
    class NestedItemValueOuter : ItemValueOuter
    {
        ItemValue outer;

        public override R.Path MakeRPath(R.Name name, R.ParamHash paramHash, ImmutableArray<R.Path> typeArgs)
        {
            var router = outer.GetRType() as R.Path.Normal;
            Debug.Assert(router != null);

            return new R.Path.Nested(router, name, paramHash, typeArgs);
        }
    }
}
