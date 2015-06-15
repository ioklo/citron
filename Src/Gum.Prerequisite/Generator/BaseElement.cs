using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Prerequisite.Generator
{
    abstract class BaseElement
    {
        string name;
        public string Name { get { return name; } }

        public List<ComponentElem> Comps { get; private set; }

        public BaseElement(string name)
        {
            this.name = name;
            Comps = new List<ComponentElem>();
        }

        public abstract void Print(IElementPrinter visitor, TextWriter Writer);
    }
}
