using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Translator.Parser
{
    public class SymbolExp : Exp
    {
        public Symbol Symbol { get; private set; }
        public string CaptureVar { get; private set; }

        public SymbolExp(Symbol symbol, string captureVar = null)
        {
            Symbol = symbol;
            CaptureVar = captureVar;
        }

        public override IEnumerable<ExpPosition> Nexts(ExpPosition pos)
        {
            yield return pos.Pop();
        }

        public override Exp GetChild(int index)
        {
            return null;
        }

        public override int GetChildCount()
        {
            return 0;
        }

        public override Ret Visit<Ret, Arg>(IExpVisitor<Ret, Arg> visitor, Arg arg)
        {
            return visitor.Visit(this, arg);
        }
    }
}
