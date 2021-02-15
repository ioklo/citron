namespace Gum.IR0
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
}
