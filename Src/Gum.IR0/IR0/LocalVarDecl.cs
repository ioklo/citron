using Gum.CompileTime;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gum.IR0
{
    public class LocalVarDecl
    {
        public ImmutableArray<VarDeclElement> Elems { get; }        

        public LocalVarDecl(ImmutableArray<VarDeclElement> elems)
        {
            Elems = elems;
        }
    }    
}