using R = Gum.IR0;

namespace Gum.Analysis
{
    // (struct, class, enum) (external/internal) (global/member) type
    public abstract partial class NormalTypeValue : TypeValue
    {
        public abstract NormalTypeValue Apply_NormalTypeValue(TypeEnv env);
        public sealed override TypeValue Apply_TypeValue(TypeEnv env)
        {
            return Apply_NormalTypeValue(env);
        }

        public sealed override R.Path GetRPath()
        {
            return GetRPath_Nested();
        }

        public abstract R.Path.Nested GetRPath_Nested();
    }
}
