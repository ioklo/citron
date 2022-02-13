using Citron.Syntax;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;
using Pretune;

namespace Citron.Syntax
{    
    public abstract record TypeExp : ISyntaxNode;
    public record IdTypeExp(string Name, ImmutableArray<TypeExp> TypeArgs) : TypeExp;
    public record MemberTypeExp(TypeExp Parent, string MemberName, ImmutableArray<TypeExp> TypeArgs) : TypeExp;
    public record NullableTypeExp(TypeExp InnerTypeExp) : TypeExp;
}