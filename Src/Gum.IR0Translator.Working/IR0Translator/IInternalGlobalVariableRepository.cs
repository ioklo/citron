using Gum.CompileTime;

namespace Gum.IR0Translator
{
    interface IInternalGlobalVariableRepository
    {
        InternalGlobalVarInfo? GetVariable(Name path);
    }
}