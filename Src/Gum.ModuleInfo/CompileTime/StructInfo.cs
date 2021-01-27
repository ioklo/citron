using Gum.Misc;
using Pretune;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Gum.CompileTime
{
    [AutoConstructor]
    public partial class StructInfo : TypeInfo, IEquatable<StructInfo?>
    {
        public override Name Name { get; }

        public override ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<Type> BaseTypes { get; }
        public override ImmutableArray<TypeInfo> MemberTypes { get; }
        public ImmutableArray<FuncInfo> MemberFuncs { get; }
        public ImmutableArray<MemberVarInfo> MemberVars { get; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as StructInfo);
        }

        public bool Equals(StructInfo? other)
        {
            return other != null &&
                   Name.Equals(other.Name) &&
                   TypeParams.SequenceEqual(other.TypeParams) &&
                   BaseTypes.SequenceEqual(other.BaseTypes) &&
                   MemberTypes.SequenceEqual(other.MemberTypes) &&
                   MemberFuncs.SequenceEqual(other.MemberFuncs) &&
                   MemberVars.SequenceEqual(other.MemberVars);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Name);

            hash.AddSequence(TypeParams);
            hash.AddSequence(BaseTypes);
            hash.AddSequence(MemberTypes);
            hash.AddSequence(MemberFuncs);
            hash.AddSequence(MemberVars);
            return hash.ToHashCode();
        }
    }
}
