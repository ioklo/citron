using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Prerequisite.Generator
{
    class SimplePrinter : IElementPrinter
    {
        public void Print(StructElem structElem, TextWriter Writer)
        {
            var header = string.Format("{0}(", structElem.Name);

            var list = string.Join(", ", structElem.Variables.Select(var =>
            {
                if (var.VarType == VarType.Single)
                    return string.Format("{0} {1}", var.Type.Name, var.Name);
                else
                    return string.Format("{0}[] {1}", var.Type.Name, var.Name);
            }));

            var footer = ")";

            Writer.WriteLine("{0}{1}{2}", header, list, footer);
        }

        public void Print(EnumElem enumElem, TextWriter Writer)
        {
            Writer.WriteLine("{0}", enumElem.Name);

            foreach (var elem in enumElem.Entries)
            {
                Writer.WriteLine("    {0}", elem);
            }
        }

        public void Print(ComponentElem compElem, TextWriter Writer)
        {
            Writer.WriteLine("{0}", compElem.Name);

            foreach (var elem in compElem.Elements)
                Writer.WriteLine("    {0}", elem.Name);
        }


        public void Print(PrimitiveElem primElem, TextWriter Writer)
        {
            Writer.WriteLine("{0}", primElem.Name);
        }

        public void PrintHeader(TextWriter writer)
        {
            
        }

        public void PrintFooter(TextWriter writer)
        {
        }
    }

}
