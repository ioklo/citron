using Gum.Collections;

namespace Gum.CompileTime
{
    public interface ICallableInfo
    {
        AccessModifier AccessModifier { get; }
        ImmutableArray<Param> Parameters { get; }
    }
}