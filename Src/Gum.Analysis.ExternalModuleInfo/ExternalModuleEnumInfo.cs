using Gum.Collections;
using Pretune;
using System;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor, ImplementIEquatable]
    partial class ExternalModuleEnumInfo : IModuleEnumInfo
    {        
        M.EnumInfo enumInfo;

        IModuleEnumElemInfo? IModuleEnumInfo.GetElem(M.Name memberName)
        {
            foreach (var elemInfo in enumInfo.ElemInfos)
                if (elemInfo.Name.Equals(memberName))
                    return new ExternalModuleEnumElemInfo(elemInfo);

            return null;
        }

        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            throw new NotImplementedException();
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            throw new NotImplementedException();
        }

        M.Name IModuleItemInfo.GetName()
        {
            throw new NotImplementedException();
        }

        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            throw new NotImplementedException();
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            throw new NotImplementedException();
        }
    }
}
