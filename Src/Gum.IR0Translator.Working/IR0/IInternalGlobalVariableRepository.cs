using Gum.CompileTime;

namespace Gum.IR0
{
    interface IInternalGlobalVariableRepository
    {
        InternalGlobalVarInfo? GetVariable(Name path);
    }
}