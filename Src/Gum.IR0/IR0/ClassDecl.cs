using Gum.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gum.Infra;

namespace Gum.IR0
{
    public partial record ClassDecl(
        AccessModifier AccessModifier,
        string Name,
        ImmutableArray<string> TypeParams,
        Path.Nested? BaseClass,
        ImmutableArray<Path.Nested> Interfaces,
        ImmutableArray<ClassMemberDecl> MemberDecls
    ) : Decl
    {
        public override void EnsurePure()
        {
            Misc.EnsurePure(MemberDecls);
        }
    }
}
