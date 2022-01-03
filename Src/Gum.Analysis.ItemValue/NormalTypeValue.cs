using R = Gum.IR0;

namespace Gum.Analysis
{
    // (struct, class, enum) (external/internal) (global/member) type
    public abstract partial class NormalTypeValue : TypeSymbol
    {
        public abstract NormalTypeValue Apply_NormalTypeValue(TypeEnv env);
        public sealed override TypeSymbol Apply(TypeEnv env)
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
