using Gum.Infra;

namespace Gum.IR0
{
    public abstract record Decl : IPure
    {
        public abstract void EnsurePure();
    }
}