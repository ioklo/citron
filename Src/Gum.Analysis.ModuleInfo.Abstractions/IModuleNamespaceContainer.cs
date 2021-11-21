using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public interface IModuleNamespaceContainer
    {
        IModuleNamespaceInfo? GetNamespace(M.Name name);
    }
}