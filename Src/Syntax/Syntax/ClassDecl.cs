using Citron.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Syntax
{
    [AutoConstructor, ImplementIEquatable]
    public partial class ClassDecl : TypeDecl
    {
        public AccessModifier? AccessModifier { get; }
        public string Name { get; }
        public ImmutableArray<TypeParam> TypeParams { get; }
        public ImmutableArray<TypeExp> BaseTypes { get; }
        public ImmutableArray<ClassMemberDecl> MemberDecls { get; }
    }
}
