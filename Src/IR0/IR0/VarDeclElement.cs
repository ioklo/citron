using Citron.Symbol;
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
        public record Normal(ITypeSymbol Type, string Name, Exp InitExp) : VarDeclElement;
        public record NormalDefault(ITypeSymbol Type, string Name) : VarDeclElement;
        public record Ref(string Name, Loc Loc) : VarDeclElement;
    }
}
