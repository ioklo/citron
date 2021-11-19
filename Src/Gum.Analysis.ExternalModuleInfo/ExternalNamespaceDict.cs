using Gum.Collections;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [ExcludeComparison]
    partial struct ExternalNamespaceDict
    {
        ImmutableDictionary<M.NamespaceName, ExternalModuleNamespaceInfo> namespaces;

        public ExternalNamespaceDict(ImmutableArray<M.NamespaceInfo> mnss)
        {
            var nssBuilder = ImmutableDictionary.CreateBuilder<M.NamespaceName, ExternalModuleNamespaceInfo>();
            foreach (var mns in mnss)
                nssBuilder.Add(mns.Name, new ExternalModuleNamespaceInfo(mns));
            namespaces = nssBuilder.ToImmutable();
        }

        public ExternalModuleNamespaceInfo? Get(M.NamespaceName name)
        {
            return namespaces.GetValueOrDefault(name);
        }
    }
}
