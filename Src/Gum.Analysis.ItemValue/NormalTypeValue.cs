
namespace Citron.Analysis
{
    // (struct, class, enum) (external/internal) (global/member) type
    public abstract partial class NormalTypeValue : ITypeSymbol
    {
        public abstract NormalTypeValue Apply_NormalTypeValue(TypeEnv env);
        public sealed override ITypeSymbol Apply(TypeEnv env)
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
