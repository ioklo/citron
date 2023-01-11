using Pretune;
using System.Collections.Generic;
using Citron.Collections;

namespace Citron.IR0
{
    public record class LocalVarDecl(ImmutableArray<VarDeclElement> Elems);
}