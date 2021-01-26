using Gum.CompileTime;

namespace Gum.IR0
{
    public interface IInternalGlobalVariableRepository
    {
        InternalGlobalVarInfo? GetVariable(Name path);
    }
}