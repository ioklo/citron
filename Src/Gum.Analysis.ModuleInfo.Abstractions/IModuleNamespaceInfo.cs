using M = Gum.CompileTime;

namespace Gum.Analysis
{
    // M.NamespaceInfo대체
    public interface IModuleNamespaceInfo : IModuleItemInfo, IModuleNamespaceContainer, IModuleTypeContainer, IModuleFuncContainer
    {
    }
}