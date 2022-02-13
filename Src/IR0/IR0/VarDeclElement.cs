using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.IR0
{
    public abstract record VarDeclElement
    {
        public record Normal(Path Type, string Name, Exp InitExp) : VarDeclElement;
        public record NormalDefault(Path Type, string Name) : VarDeclElement;
        public record Ref(string Name, Loc Loc) : VarDeclElement;
    }
}
