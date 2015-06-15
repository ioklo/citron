using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Prerequisite.Generator
{
    class PrimitiveElem : BaseElement
    {
        public PrimitiveElem(string name)
            : base(name)
        { }


        public override void Print(IElementPrinter visitor, TextWriter writer)
        {
            visitor.Print(this, writer);
        }
    }
}
