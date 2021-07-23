using Pretune;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using System.Text;
using Gum.Infra;

namespace Gum.CompileTime
{
    [AutoConstructor, ImplementIEquatable]
    public partial class StructInfo : TypeInfo
    {
        public override Name Name { get; }

        public override ImmutableArray<string> TypeParams { get; }
        public Type? BaseType { get; }
        public ImmutableArray<Type> Interfaces { get; }
        public ImmutableArray<TypeInfo> MemberTypes { get; }
        public ImmutableArray<FuncInfo> MemberFuncs { get; }
        public ImmutableArray<MemberVarInfo> MemberVars { get; }

        public ImmutableArray<ConstructorInfo> Constructors { get; }

        public override TypeInfo? GetMemberType(string name, int typeParamCount)
        {
            foreach (var memberType in MemberTypes)
                if (memberType.TypeParams.Length == typeParamCount && memberType.Name.Equals(name))
                    return memberType;

            return null;
        }
    }
}
