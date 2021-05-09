using System.Diagnostics;

namespace Gum.Infra
{
    // Mark for logically pure class
    public interface IPure
    {
        void EnsurePure();
    }
}