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
        public IReadOnlyList<BaseElement> Elements { get { return elements; } }

        public ComponentElem(string name)
            : base(name)
        {
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
