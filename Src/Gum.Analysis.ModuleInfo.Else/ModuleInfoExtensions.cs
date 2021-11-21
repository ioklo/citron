using System.Diagnostics;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public static class ModuleInfoExtensions
    {
        public static IModuleNamespaceInfo? GetNamespace(this IModuleNamespaceContainer namespaceContainer, M.NamespacePath namespacePath)
        {
            Debug.Assert(!namespacePath.IsRoot);

            IModuleNamespaceInfo? curNamespace = null;
            foreach (var entry in namespacePath.Entries)
            {
                curNamespace = namespaceContainer.GetNamespace(entry);

                if (curNamespace == null) return null;
                namespaceContainer = curNamespace;
            }

            return curNamespace;
        }
    }
}