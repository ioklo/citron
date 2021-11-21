using Gum.Analysis;
using Gum.Collections;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor]
    partial class InternalModuleNamespaceInfo : IModuleNamespaceInfo
    {
        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            throw new System.NotImplementedException();
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            throw new System.NotImplementedException();
        }

        M.Name IModuleItemInfo.GetName()
        {
            throw new System.NotImplementedException();
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return ImmutableArray<string>.Empty;
        }

        IModuleNamespaceInfo? IModuleNamespaceContainer.GetNamespace(M.Name name)
        {
            throw new System.NotImplementedException();
        }

        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            throw new System.NotImplementedException();
        }
    }
}