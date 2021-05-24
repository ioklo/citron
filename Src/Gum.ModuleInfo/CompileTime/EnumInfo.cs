using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Pretune;

namespace Gum.CompileTime
{
    [AutoConstructor]
    public partial struct EnumElemFieldInfo
    {
        public Type Type { get; }
        public string Name { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class EnumElemInfo : TypeInfo
    {
        public override Name Name { get; }
        public override ImmutableArray<string> TypeParams => default;
        public ImmutableArray<EnumElemFieldInfo> FieldInfos { get; }
        public override TypeInfo? GetMemberType(string name, int typeParamCount) => null;
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class EnumInfo : TypeInfo
    {
        public override Name Name { get; }
        public override ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<EnumElemInfo> elemInfos { get; }
        
        public override TypeInfo? GetMemberType(string name, int typeParamCount)
        {
            if (typeParamCount != 0) return null;

            foreach (var elemInfo in elemInfos)
                if (elemInfo.Name.Equals(name))
                    return elemInfo;

            return null;
        }
    }
}
