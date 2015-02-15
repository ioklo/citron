using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Translator.Parser
{
    public class PlusExp : Exp
    {
        Exp e;
        public Exp E { get { return e; } }

        public PlusExp(Exp e)
        {
            this.e = e;
        }

        public override IEnumerable<ExpPosition> Nexts(ExpPosition pos)
        {
            if( pos.Index == 0 )
            {
                yield return pos.StepOut().Push(e, 0);
            }
            else
            {
                yield return pos.Push(e, 0);
                yield return pos.Pop();
            }
        }

        public override Exp GetChild(int index)
        {
            if (index == 0) return e;
            return null;
        }

        public override int GetChildCount()
        {
            return 1;
        }

        public override Ret Visit<Ret, Arg>(IExpVisitor<Ret, Arg> visitor, Arg arg)
        {
            return visitor.Visit(this, arg);
        }
    }
}
