using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using Pretune;
using Gum.Infra;

namespace Gum.IR0
{
    public record SequenceFuncDecl(
        ImmutableArray<Decl> Decls,
        Name Name,
        bool IsThisCall,
        Path YieldType,
        ImmutableArray<string> TypeParams,
        ImmutableArray<Param> Parameters,
        Stmt Body
    ) : Decl
    {
        public override void EnsurePure()
        {
            Misc.EnsurePure(Decls);
        }
    }
}
