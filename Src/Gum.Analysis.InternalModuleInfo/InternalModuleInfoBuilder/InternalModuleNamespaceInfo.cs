using Gum.Analysis;
using Gum.Collections;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor]
    partial class InternalModuleNamespaceInfo : IModuleNamespaceInfo
    {
        IModuleFuncDecl? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            throw new System.NotImplementedException();
        }

        ImmutableArray<IModuleFuncDecl> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            throw new System.NotImplementedException();
        }

        M.Name IModuleItemDecl.GetName()
        {
            throw new System.NotImplementedException();
        }

        ImmutableArray<string> IModuleItemDecl.GetTypeParams()
        {
            return ImmutableArray<string>.Empty;
        }

        IModuleNamespaceInfo? IModuleNamespaceContainer.GetNamespace(M.Name name)
        {
            throw new System.NotImplementedException();
        }

        IModuleTypeDecl? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            throw new System.NotImplementedException();
        }
    }
}