using Gum.Collections;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    public interface IModuleDriver
    {
        ImmutableArray<(R.ModuleName ModuleName, IItemContainer Container)> GetRootContainers();
    }
}