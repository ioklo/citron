using Pretune;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Gum.CompileTime
{
    [AutoConstructor]
    public partial class StructInfo : TypeInfo
    {
        public override Name Name { get; }

        public override ImmutableArray<string> TypeParams { get; }
        public Type BaseType { get; }
        public override ImmutableArray<TypeInfo> MemberTypes { get; }
        public ImmutableArray<FuncInfo> MemberFuncs { get; }
        public ImmutableArray<MemberVarInfo> MemberVars { get; }
    }
}
