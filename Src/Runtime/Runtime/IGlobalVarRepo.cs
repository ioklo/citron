using Citron.CompileTime;

namespace Citron.Runtime
{
    public interface IGlobalVarRepo
    {
        Value GetValue(ItemId varId);
        void SetValue(ItemId varId, Value value);
    }
}