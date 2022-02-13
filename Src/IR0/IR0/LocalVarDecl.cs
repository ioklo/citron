using Citron.CompileTime;
using Pretune;
using System.Collections.Generic;
using Citron.Collections;

namespace Citron.IR0
{
    public record LocalVarDecl(ImmutableArray<VarDeclElement> Elems);
}