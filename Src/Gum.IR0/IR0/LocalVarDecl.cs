using Gum.CompileTime;
using Pretune;
using System.Collections.Generic;
using Gum.Collections;

namespace Gum.IR0
{
    public record LocalVarDecl(ImmutableArray<VarDeclElement> Elems);
}