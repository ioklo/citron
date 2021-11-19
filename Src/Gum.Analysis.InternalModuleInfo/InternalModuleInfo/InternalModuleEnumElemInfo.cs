using Gum.Analysis;
using Gum.Collections;
using Pretune;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor, ImplementIEquatable]
    partial class InternalModuleEnumElemInfo : IModuleEnumElemInfo
    {
        M.Name name;
        ImmutableArray<IModuleMemberVarInfo> fieldInfos;

        ImmutableArray<IModuleMemberVarInfo> IModuleEnumElemInfo.GetFieldInfos()
        {
            return fieldInfos;
        }

        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return null;
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            return default;
        }

        M.Name IModuleItemInfo.GetName()
        {
            return name;
        }

        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            return null;
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return default;
        }

        bool IModuleEnumElemInfo.IsStandalone()
        {
            return fieldInfos.Length == 0;
        }
    }
}