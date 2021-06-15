using Gum.Syntax;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using System.Text;
using Pretune;

namespace Gum.Syntax
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct EnumElementField
    {
        public TypeExp Type { get; }
        public string Name { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class EnumDeclElement : ISyntaxNode
    {
        public string Name { get; }
        public ImmutableArray<EnumElementField> Fields { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class EnumDecl : TypeDecl
    {
        public override string Name { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<EnumDeclElement> Elems { get; }

        public override int TypeParamCount { get => TypeParams.Length; }
    }
}
