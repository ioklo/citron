using Citron.Syntax;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;
using Pretune;
using System.Diagnostics;

namespace Citron.Syntax
{
    public abstract record class TypeExp : ISyntaxNode;
    public record class IdTypeExp(string Name, ImmutableArray<TypeExp> TypeArgs) : TypeExp;
    public record class MemberTypeExp(TypeExp Parent, string MemberName, ImmutableArray<TypeExp> TypeArgs) : TypeExp;
    public record class NullableTypeExp(TypeExp InnerTypeExp) : TypeExp;
    public record class RefTypeExp(TypeExp InnerTypeExp) : TypeExp;
}