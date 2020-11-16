using Gum.CompileTime;

namespace Gum.Runtime
{
    public interface IGlobalVarRepo
    {
        Value GetValue(ItemId varId);
        void SetValue(ItemId varId, Value value);
    }
}