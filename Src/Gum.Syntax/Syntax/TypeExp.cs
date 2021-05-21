using Gum.Syntax;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using Pretune;

namespace Gum.Syntax
{    
    public abstract class TypeExp : ISyntaxNode
    {
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class IdTypeExp : TypeExp
    {
        public string Name { get; }
        public ImmutableArray<TypeExp> TypeArgs { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class MemberTypeExp : TypeExp
    {
        public TypeExp Parent { get; }
        public string MemberName { get; }
        public ImmutableArray<TypeExp> TypeArgs { get; }
    }
}