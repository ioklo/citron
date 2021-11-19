using M = Gum.CompileTime;

namespace Gum.Analysis
{
    // M.FuncInfo 대체
    public interface IModuleFuncInfo : IModuleCallableInfo
    {
        bool IsInstanceFunc();
        bool IsSequenceFunc();
        M.Type GetReturnType();
        bool IsInternal();
    }
}