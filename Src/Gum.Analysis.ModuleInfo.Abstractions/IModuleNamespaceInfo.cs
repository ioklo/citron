using M = Gum.CompileTime;

namespace Gum.Analysis
{
    // M.NamespaceInfo대체
    public interface IModuleNamespaceInfo : IModuleNamespaceContainer, IModuleTypeContainer, IModuleFuncContainer
    {
        M.NamespaceName GetName();
    }
}