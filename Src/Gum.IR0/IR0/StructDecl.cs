﻿using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using Gum.Infra;
using Pretune;

namespace Gum.IR0
{
    public partial record StructDecl(
        AccessModifier AccessModifier,
        string Name,
        ImmutableArray<string> TypeParams,
        ImmutableArray<Path> BaseTypes,
        ImmutableArray<StructMemberDecl> MemberDecls
    ) : Decl
    {   
        public override void EnsurePure()
        {
            Misc.EnsurePure(TypeParams);
            Misc.EnsurePure(BaseTypes);
        }
    }
}
