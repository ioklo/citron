using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Translator.Parser
{
    public class SequenceExp : Exp
    {
        List<Exp> children;
        public IReadOnlyList<Exp> Children { get { return children; } }

        public SequenceExp(params Exp[] children)
        {
            this.children = new List<Exp>(children);
        }

        public override IEnumerable<ExpPosition> Nexts(ExpPosition pos)
        {
            if (pos.Index < children.Count)
            {
                var curChild = children[pos.Index];
                yield return pos.StepOut().Push(curChild, 0);
            }
            else
                yield return pos.Pop();
        }

        public override Exp GetChild(int index)
        {
            return children[index];
        }

        public override int GetChildCount()
        {
            return children.Count;
        }

        public override Ret Visit<Ret, Arg>(IExpVisitor<Ret, Arg> visitor, Arg arg)
        {
            return visitor.Visit(this, arg);
        }
    }
}
