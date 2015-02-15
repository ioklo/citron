using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Translator.Parser
{
    public class ExpPosition
    {
        public static ExpPosition Empty { get; private set; }
        static ExpPosition()
        {
            Empty = new ExpPosition(null, 0, null);
        }

        private ExpPosition Parent { get; set; }
        public Exp Exp { get; private set; }
        public int Index { get; private set; }        

        public ExpPosition(Exp exp)
        {
            Exp = exp;
            Index = 0;

            Parent = Empty;
        }

        private ExpPosition(Exp exp, int index, ExpPosition parent)
        { 
            Exp = exp; 
            Index = index;            
            Parent = parent;
        }

        public ExpPosition Pop()
        {
            return Parent;
        }

        public ExpPosition Push(Exp exp, int index)
        {
            return new ExpPosition(exp, index, this);
        }

        public ExpPosition StepOut()
        {
            return Parent.Push(Exp, Index + 1);
        }

        public IEnumerable<ExpPosition> StepIns()
        {
            return Exp.Nexts(this);
        }

        public override bool Equals(Object o)
        {
            ExpPosition other = o as ExpPosition;
            if (other == null) return false;

            if (Exp != other.Exp) return false;
            if (Index != other.Index) return false;

            if (Parent == null && other.Parent != null) return false;
            if (Parent != null && !Parent.Equals(other.Parent)) return false;

            return true;
        }

        public override string ToString()
        {
            if (Parent != null)
                return string.Format("{0}, ({1}, {2})", Parent, Exp, Index);

            if (Exp != null)
                return string.Format("({0}, {1})", Exp, Index);

            return string.Format("(Empty)");
        }

        public override int GetHashCode()
        {
            return new { Parent = Parent, Exp = Exp, Index = Index }.GetHashCode();
        }

        internal bool IsAvailable(Symbol symbol)
        {
            var ruleExp = Exp as SymbolExp;
            if (ruleExp == null) return false;

            return ruleExp.Symbol == symbol;
        }
    }
}
