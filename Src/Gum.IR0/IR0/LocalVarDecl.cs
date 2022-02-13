using Gum.CompileTime;
using Pretune;
using System.Collections.Generic;
using Gum.Collections;

namespace Citron.IR0
{
    public record LocalVarDecl(ImmutableArray<VarDeclElement> Elems);
}