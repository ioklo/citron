using Gum.Collections;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    partial class InternalModuleEnumInfo : IModuleEnumInfo
    {
        M.Name name;
        ImmutableArray<string> typeParams;
        ImmutableDictionary<M.Name, IModuleEnumElemInfo> elemDict;

        public InternalModuleEnumInfo(M.Name name, ImmutableArray<string> typeParams, ImmutableArray<InternalModuleEnumElemInfo> elemInfos)
        {
            this.name = name;
            this.typeParams = typeParams;

            var builder = ImmutableDictionary.CreateBuilder<M.Name, IModuleEnumElemInfo>();
            foreach(var elemInfo in elemInfos)
                builder.Add(((IModuleEnumElemInfo)elemInfo).GetName(), elemInfo);
            elemDict = builder.ToImmutable();
        }


        IModuleEnumElemInfo? IModuleEnumInfo.GetElem(M.Name memberName)
        {
            return elemDict.GetValueOrDefault(memberName);
        }

        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return null;
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            return ImmutableArray<IModuleFuncInfo>.Empty;
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
            return typeParams;
        }
    }
}