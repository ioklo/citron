using Gum.Infra;

namespace Gum.IR0
{
    public abstract class Decl : IPure
    {
        public abstract void EnsurePure();
    }
}