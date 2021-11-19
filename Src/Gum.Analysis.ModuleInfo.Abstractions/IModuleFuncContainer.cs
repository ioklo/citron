using Gum.Collections;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public interface IModuleFuncContainer
    {
        IModuleFuncInfo? GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes);  // find exact one
        ImmutableArray<IModuleFuncInfo> GetFuncs(M.Name name, int minTypeParamCount); // find all possibilities
    }
}