using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0
{
    public abstract record VarDeclElement
    {
        public record Normal(Path Type, string Name, Exp? InitExp) : VarDeclElement;
        public record Ref(string Name, Loc Loc) : VarDeclElement;
    }
}
