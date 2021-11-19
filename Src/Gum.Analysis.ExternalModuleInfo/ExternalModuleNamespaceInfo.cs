using Gum.Collections;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [ImplementIEquatable]
    partial class ExternalModuleNamespaceInfo : IModuleNamespaceInfo
    {
        M.NamespaceName name;
        ExternalNamespaceDict namespaceDict;
        ModuleTypeDict typeDict;
        ModuleFuncDict funcDict;

        public ExternalModuleNamespaceInfo(M.NamespaceInfo mns)
        {
            name = mns.Name;
            namespaceDict = new ExternalNamespaceDict(mns.Namespaces);
            typeDict = new ModuleTypeDict(mns.Types);
            funcDict = new ModuleFuncDict(mns.Funcs);
        }

        M.NamespaceName IModuleNamespaceInfo.GetName()
        {
            return name;
        }

        IModuleNamespaceInfo? IModuleNamespaceContainer.GetNamespace(M.NamespaceName name)
        {
            return namespaceDict.Get(name);
        }

        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            return typeDict.Get(name, typeParamCount);
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            return funcDict.Get(name, minTypeParamCount);
        }

        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return funcDict.Get(name, typeParamCount, paramTypes);
        }
    }
}
