using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Prerequisite.Generator
{
    interface IElementPrinter
    {
        void PrintHeader(TextWriter writer);
        void PrintFooter(TextWriter writer);

        void Print(StructElem structElem, TextWriter Writer);
        void Print(EnumElem enumElem, TextWriter Writer);
        void Print(ComponentElem compElem, TextWriter Writer);
        void Print(PrimitiveElem primitiveElem, TextWriter Writer);
    }
}
