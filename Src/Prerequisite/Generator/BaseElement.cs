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
        public IEnumerable<ComponentElem> AllComps
        {
            get
            {
                return InnerAllComps().Distinct();
            }
        }

        private IEnumerable<ComponentElem> InnerAllComps()
        {
            foreach (var comp in Comps)
            {
                yield return comp;
                foreach( var child in comp.AllComps )
                {
                    yield return child;
                }
            }
        }

        public BaseElement(string name)
        {
            this.name = name;
            Comps = new List<ComponentElem>();
        }

        public abstract void Print(IElementPrinter visitor, TextWriter Writer);
    }
}
