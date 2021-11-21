using Gum.Collections;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [ImplementIEquatable]
    partial class ExternalModuleNamespaceInfo : IModuleNamespaceInfo
    {
        M.Name name;
        ExternalNamespaceDict namespaceDict;
        ModuleTypeDict typeDict;
        ModuleFuncDict funcDict;

        public ExternalModuleNamespaceInfo(M.NamespaceInfo mns)
        {
            name = mns.Name;
            namespaceDict = new ExternalNamespaceDict(mns.Namespaces);
            typeDict = ExternalModuleMisc.MakeModuleTypeDict(mns.Types);
            funcDict = ExternalModuleMisc.MakeModuleFuncDict(mns.Funcs);
        }

        M.Name IModuleItemInfo.GetName()
        {
            return name;
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return ImmutableArray<string>.Empty;
        }   

        IModuleNamespaceInfo? IModuleNamespaceContainer.GetNamespace(M.Name name)
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
