using Gum.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Syntax
{
    [AutoConstructor, ImplementIEquatable]
    public partial class ClassDecl : TypeDecl
    {
        public AccessModifier? AccessModifier { get; }
        public string Name { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<TypeExp> BaseTypes { get; }
        public ImmutableArray<ClassMemberDecl> MemberDecls { get; }
    }
}
