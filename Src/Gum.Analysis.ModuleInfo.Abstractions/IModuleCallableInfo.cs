using Gum.Collections;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public interface IModuleCallableInfo : IModuleItemInfo
    {
        M.AccessModifier GetAccessModifier();
        ImmutableArray<M.Param> GetParameters();
        M.ParamTypes GetParamTypes();
    }
}