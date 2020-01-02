using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

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

    public interface IEnumInfo : ITypeInfo
    {
        bool GetElemInfo(string idName, [NotNullWhen(returnValue: true)] out EnumElemInfo? outElemInfo);
        EnumElemInfo GetDefaultElemInfo();
    }
}