using Citron.Syntax;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;
using System.Text;
using Pretune;

namespace Citron.Syntax
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct EnumElemMemberVarDecl : ISyntaxNode
    {
        public TypeExp Type { get; }
        public string Name { get; }
    }
    
    [AutoConstructor, ImplementIEquatable]
    public partial class EnumElemDecl : ISyntaxNode
    {
        public string Name { get; }
        public ImmutableArray<EnumElemMemberVarDecl> MemberVars { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class EnumDecl : TypeDecl
    {
        public AccessModifier? AccessModifier { get; }
        public string Name { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<EnumElemDecl> Elems { get; }
    }
}
