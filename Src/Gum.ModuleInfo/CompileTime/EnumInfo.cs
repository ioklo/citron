using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Gum.CompileTime
{
    public struct EnumElemFieldInfo
    {
        public TypeValue TypeValue { get; }
        public string Name { get; }

        public EnumElemFieldInfo(TypeValue typeValue, string name)
        {
            TypeValue = typeValue;
            Name = name;
        }
    }

    public struct EnumElemInfo
    {
        public string Name { get; }
        public ImmutableArray<EnumElemFieldInfo> FieldInfos { get; }

        public EnumElemInfo(string name, IEnumerable<EnumElemFieldInfo> fieldInfos)
        {
            Name = name;
            FieldInfos = fieldInfos.ToImmutableArray();
        }
    }

    public class EnumInfo : TypeInfo
    {
        ImmutableArray<string> typeParams;
        ImmutableDictionary<string, EnumElemInfo> elemInfosByName;
        EnumElemInfo defaultElemInfo;

        public EnumInfo(
            ItemId id,
            IEnumerable<string> typeParams,
            IEnumerable<EnumElemInfo> elemInfos)
            : base(id, typeParams, null, Array.Empty<ItemInfo>())
        {   
            this.typeParams = typeParams.ToImmutableArray();

            defaultElemInfo = elemInfos.First();
            this.elemInfosByName = elemInfos.ToImmutableDictionary(elemInfo => elemInfo.Name);
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
