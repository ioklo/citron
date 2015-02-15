using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Translator.Parser
{
    public abstract class Exp
    {
        public abstract IEnumerable<ExpPosition> Nexts(ExpPosition pos);

        public abstract int GetChildCount();
        public abstract Exp GetChild(int index);

        public abstract Ret Visit<Ret, Arg>(IExpVisitor<Ret, Arg> visitor, Arg arg);
    }
}
