using Gum.Syntax;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using Pretune;

namespace Gum.Syntax
{
    // reference 비교를 하는건 의도적인 것임
    public abstract class TypeExp : ISyntaxNode
    {
    }

    [AutoConstructor]
    public partial class IdTypeExp : TypeExp
    {
        public string Name { get; }
        public ImmutableArray<TypeExp> TypeArgs { get; }
    }

    [AutoConstructor]
    public partial class MemberTypeExp : TypeExp
    {
        public TypeExp Parent { get; }
        public string MemberName { get; }
        public ImmutableArray<TypeExp> TypeArgs { get; }
    }
}