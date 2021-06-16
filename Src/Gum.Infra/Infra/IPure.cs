using System.Diagnostics;

namespace Gum.Infra
{
    // Mark for logically pure class (immutability of 'observation' properties)
    public interface IPure
    {
        void EnsurePure();
    }    
}