using Gum.Collections;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public interface IModuleItemInfo
    {
        M.Name GetName();
        ImmutableArray<string> GetTypeParams();
    }
}