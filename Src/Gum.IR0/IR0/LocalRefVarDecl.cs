using Gum.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0
{
    public record LocalRefVarDecl(ImmutableArray<RefVarDeclElement> Elems);
}
