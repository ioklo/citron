using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using Gum.Infra;
using Pretune;

namespace Gum.Syntax
{
    [AutoConstructor, ImplementIEquatable]
    public partial class StructDecl : TypeDecl
    {   
        public AccessModifier? AccessModifier { get; }
        public override string Name { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<TypeExp> BaseTypes { get; }
        public ImmutableArray<StructDeclElement> Elems { get; }

        public override int TypeParamCount { get => TypeParams.Length; }
    }
}
