using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Gum.CompileTime
{
    public class EnumInfo : IEnumInfo
    {
        public ModuleItemId? OuterTypeId { get; }
        public ModuleItemId TypeId { get; }

        ImmutableArray<string> typeParams;
        ImmutableDictionary<string, EnumElemInfo> elemInfosByName;
        EnumElemInfo defaultElemInfo;

        public EnumInfo(
            ModuleItemId? outerTypeId,
            ModuleItemId typeId,
            IEnumerable<string> typeParams,
            IEnumerable<EnumElemInfo> elemInfos)
        {
            OuterTypeId = outerTypeId;
            TypeId = typeId;
            this.typeParams = typeParams.ToImmutableArray();

            defaultElemInfo = elemInfos.First();
            this.elemInfosByName = elemInfos.ToImmutableDictionary(elemInfo => elemInfo.Name);
        }

        public IReadOnlyList<string> GetTypeParams()
        {
            return typeParams;
        }

        public TypeValue? GetBaseTypeValue()
        {
            return null;
        }

        public bool GetMemberTypeId(string name, [NotNullWhen(true)] out ModuleItemId? outTypeId)
        {
            outTypeId = null;
            return false;
        }

        public bool GetMemberFuncId(Name memberFuncId, [NotNullWhen(true)] out ModuleItemId? outFuncId)
        {
            outFuncId = null;
            return false;
        }

        public bool GetMemberVarId(Name name, [NotNullWhen(true)] out ModuleItemId? outVarId)
        {
            outVarId = null;
            return false;
        }

        public bool GetElemInfo(string idName, [NotNullWhen(true)] out EnumElemInfo? outElemInfo)
        {
            if (elemInfosByName.TryGetValue(idName, out var elemInfo))
            {
                outElemInfo = elemInfo;
                return true;
            }
            else
            {
                outElemInfo = null;
                return false;
            }
        }

        public EnumElemInfo GetDefaultElemInfo()
        {
            return defaultElemInfo;
        }
    }
}
