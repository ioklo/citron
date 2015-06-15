using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Prerequisite.Generator
{
    class StructElem : BaseElement
    {
        List<StructVar> variables = new List<StructVar>();
        public IReadOnlyList<StructVar> Variables { get { return variables; } }

        public StructElem(string name)
            : base(name)
        {
        }

        internal StructElem Var(BaseElement element, string name)
        {
            variables.Add(new StructVar(VarType.Single, element, name));
            return this;
        }

        internal StructElem Vars(BaseElement element, string name)
        {
            variables.Add(new StructVar(VarType.List, element, name));
            return this;
        }

        public override void Print(IElementPrinter visitor, TextWriter writer)
        {
            visitor.Print(this, writer);
        }
    }

}
