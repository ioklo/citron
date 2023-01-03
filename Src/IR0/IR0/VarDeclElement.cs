using Citron.Symbol;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.IR0
{
    public abstract record class VarDeclElement
    {
        public record class Normal(ITypeSymbol Type, string Name, Exp InitExp) : VarDeclElement;
        public record class NormalDefault(ITypeSymbol Type, string Name) : VarDeclElement;
        public record class Ref(string Name, Loc Loc) : VarDeclElement;
    }
}
