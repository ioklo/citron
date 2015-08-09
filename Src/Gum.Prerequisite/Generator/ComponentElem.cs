using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Prerequisite.Generator
{
    class ComponentElem : BaseElement
    {
        List<BaseElement> elements = new List<BaseElement>();
        List<ElementVar> variables = new List<ElementVar>();

        public IReadOnlyList<BaseElement> Elements { get { return elements; } }
        public IReadOnlyList<ElementVar> Variables { get { return variables; } }

        public ComponentElem(string name)
            : base(name)
        {
        }

        internal ComponentElem Var(BaseElement element, string name)
        {
            variables.Add(new ElementVar(VarType.Single, element, name));
            return this;
        }

        internal ComponentElem Vars(BaseElement element, string name)
        {
            variables.Add(new ElementVar(VarType.List, element, name));
            return this;
        }

        internal ComponentElem Add(BaseElement element)
        {
            elements.Add(element);
            element.Comps.Add(this);
            return this;
        }

        public override void Print(IElementPrinter visitor, TextWriter writer)
        {
            visitor.Print(this, writer);
        }
    }
}
