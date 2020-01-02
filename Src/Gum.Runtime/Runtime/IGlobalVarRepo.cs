using Gum.CompileTime;

namespace Gum.Runtime
{
    public interface IGlobalVarRepo
    {
        Value GetValue(ModuleItemId varId);
        void SetValue(ModuleItemId varId, Value value);
    }
}