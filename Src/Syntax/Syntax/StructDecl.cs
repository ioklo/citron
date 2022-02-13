using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Text;
using Citron.Infra;
using Pretune;

namespace Citron.Syntax
{
    [AutoConstructor, ImplementIEquatable]
    public partial class StructDecl : TypeDecl
    {   
        public AccessModifier? AccessModifier { get; }
        public string Name { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<TypeExp> BaseTypes { get; }
        public ImmutableArray<StructMemberDecl> MemberDecls { get; }
    }
}
