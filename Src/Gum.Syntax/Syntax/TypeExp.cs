using Gum.Syntax;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using Pretune;

namespace Gum.Syntax
{
    public abstract record TypeExp : ISyntaxNode;
    public record IdTypeExp(string Name, ImmutableArray<TypeExp> TypeArgs) : TypeExp;
    public record MemberTypeExp(TypeExp Parent, string MemberName, ImmutableArray<TypeExp> TypeArgs) : TypeExp;
}