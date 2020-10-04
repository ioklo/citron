using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Gum.Infra;

namespace Gum.IR0
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

        public override bool Equals(object? obj)
        {
            return obj is StructDecl decl &&
                   AccessModifier == decl.AccessModifier &&
                   Name == decl.Name &&
                   SeqEqComparer.Equals(TypeParams, decl.TypeParams) &&
                   SeqEqComparer.Equals(BaseTypes, decl.BaseTypes) &&
                   SeqEqComparer.Equals(Elems, decl.Elems);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(AccessModifier);
            hashCode.Add(Name);
            SeqEqComparer.AddHash(ref hashCode, TypeParams);
            SeqEqComparer.AddHash(ref hashCode, BaseTypes);
            SeqEqComparer.AddHash(ref hashCode, Elems);

            return hashCode.ToHashCode();
        }

        public static bool operator ==(StructDecl? left, StructDecl? right)
        {
            return EqualityComparer<StructDecl?>.Default.Equals(left, right);
        }

        public static bool operator !=(StructDecl? left, StructDecl? right)
        {
            return !(left == right);
        }
    }
}
