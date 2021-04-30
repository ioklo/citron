using Pretune;
using M = Gum.CompileTime;

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
    }

    // 최상위 
    [AutoConstructor]
    class RootValue : ItemValue
    {
        M.ModuleName moduleName;
        M.NamespacePath namespacePath;
    }
}
