using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using Gum.Infra;

namespace Gum.Syntax
{
    public partial class StructDecl : TypeDecl
    {
        public AccessModifier AccessModifier { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<TypeExp> BaseTypes { get; }
        public ImmutableArray<StructDeclElement> Elems { get; }

        public override int TypeParamCount { get => TypeParams.Length; }

        public StructDecl(
            AccessModifier accessModifier,
            string name,
            ImmutableArray<string> typeParams,
            ImmutableArray<TypeExp> baseTypes,
            ImmutableArray<StructDeclElement> elems)
            : base(name)
        {
            AccessModifier = accessModifier;
            TypeParams = typeParams;
            BaseTypes = baseTypes;
            Elems = elems;
        }
    }
}
