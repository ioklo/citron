using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Gum.Infra;

namespace Gum.Syntax
{
    public partial class StructDecl : ISyntaxNode
    {
        public AccessModifier AccessModifier { get; }
        public string Name { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<TypeExp> BaseTypes { get; }
        public ImmutableArray<Element> Elems { get; }

        public StructDecl(
            AccessModifier accessModifier,
            string name,
            IEnumerable<string> typeParams,
            IEnumerable<TypeExp> baseTypes,
            IEnumerable<Element> elems)
        {
            AccessModifier = accessModifier;
            Name = name;
            TypeParams = typeParams.ToImmutableArray();
            BaseTypes = baseTypes.ToImmutableArray();
            Elems = elems.ToImmutableArray();
        }
    }
}
