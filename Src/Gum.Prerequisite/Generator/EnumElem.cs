using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Prerequisite.Generator
{
    class EnumElem : BaseElement
    {
        List<string> entries = new List<string>();

        public EnumElem(string name)
            : base(name)
        {

        }

        internal EnumElem Add(string entry)
        {
            entries.Add(entry);
            return this;
        }

        public override void Print(IElementPrinter visitor, TextWriter writer)
        {
            visitor.Print(this, writer);
        }

        public IEnumerable<string> Entries { get { return entries; } }
    }
}
