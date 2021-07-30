using Gum.Collections;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
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

        M.NamespaceName IModuleNamespaceInfo.GetName()
        {
            throw new System.NotImplementedException();
        }

        IModuleNamespaceInfo? IModuleNamespaceContainer.GetNamespace(M.NamespaceName name)
        {
            throw new System.NotImplementedException();
        }

        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            throw new System.NotImplementedException();
        }
    }
}