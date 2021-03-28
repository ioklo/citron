using Gum.CompileTime;
using Pretune;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial class LocalVarDecl
    {
        public ImmutableArray<VarDeclElement> Elems { get; }
    }    
}