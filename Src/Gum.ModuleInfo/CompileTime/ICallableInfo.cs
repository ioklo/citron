using Gum.Collections;

namespace Gum.CompileTime
{
    public interface ICallableInfo
    {
        ImmutableArray<Param> Parameters { get; }
    }
}