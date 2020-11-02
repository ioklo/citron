using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gum.Syntax
{
    public abstract class TypeExp : ISyntaxNode
    {   
    }
 
    public class IdTypeExp : TypeExp
    {
        public string Name { get; }
        public ImmutableArray<TypeExp> TypeArgs { get; }
        public IdTypeExp(string name, IEnumerable<TypeExp> typeArgs) { Name = name; TypeArgs = typeArgs.ToImmutableArray(); }
        public IdTypeExp(string name, params TypeExp[] typeArgs) { Name = name; TypeArgs = ImmutableArray.Create(typeArgs); }
    }

    public class MemberTypeExp : TypeExp
    {
        public TypeExp Parent { get; }        
        public string MemberName { get; }
        public ImmutableArray<TypeExp> TypeArgs { get; }

        public MemberTypeExp(TypeExp parent, string memberName, IEnumerable<TypeExp> typeArgs)
        {
            Parent = parent;
            MemberName = memberName;
            TypeArgs = typeArgs.ToImmutableArray();
        }
    }
}