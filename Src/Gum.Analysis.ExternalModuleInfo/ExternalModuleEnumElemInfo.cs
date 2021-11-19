using Gum.Collections;
using Pretune;
using System;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    [AutoConstructor, ImplementIEquatable]
    partial class ExternalModuleEnumElemInfo : IModuleEnumElemInfo
    {
        M.EnumElemInfo elemInfo;
        
        ImmutableArray<IModuleMemberVarInfo> IModuleEnumElemInfo.GetFieldInfos()
        {
            var builder = ImmutableArray.CreateBuilder<IModuleMemberVarInfo>(elemInfo.FieldInfos.Length);

            foreach (var fieldInfo in elemInfo.FieldInfos)
                builder.Add(new ExternalModuleMemberVarInfo(fieldInfo));

            return builder.MoveToImmutable();
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
            return elemInfo.Name;
        }

        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            throw new NotImplementedException();
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            throw new NotImplementedException();
        }

        bool IModuleEnumElemInfo.IsStandalone()
        {
            return elemInfo.FieldInfos.Length == 0;
        }
    }
}
