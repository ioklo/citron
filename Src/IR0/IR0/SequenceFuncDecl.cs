using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Text;
using Pretune;
using Citron.Infra;

namespace Citron.IR0
{
    public record SequenceFuncDecl(
        ImmutableArray<CallableMemberDecl> CallableMemberDecls,
        Name Name,
        bool IsThisCall,
        Path YieldType,
        ImmutableArray<string> TypeParams,
        ImmutableArray<Param> Parameters,
        Stmt Body
    ) : FuncDecl
    {
    }
}
