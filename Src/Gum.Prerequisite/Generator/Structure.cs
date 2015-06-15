using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Prerequisite.Generator
{
    class Structure
    {
        Dictionary<string, BaseElement> elements = new Dictionary<string, BaseElement>();

        public StructElem CreateStruct(string name)
        {
            var structElem = new StructElem(name);
            elements.Add(name, structElem);
            return structElem;
        }

        public EnumElem CreateEnum(string name)
        {
            var enumElem = new EnumElem(name);
            elements.Add(name, enumElem);
            return enumElem;
        }

        public ComponentElem CreateComponent(string name)
        {
            var compElem = new ComponentElem(name);
            elements.Add(name, compElem);
            return compElem;
        }

        public PrimitiveElem CreatePrimitive(string name)
        {
            var primElem = new PrimitiveElem(name);
            elements.Add(name, primElem);
            return primElem;
        }

        public void Print(IElementPrinter printer, TextWriter writer)
        {
            printer.PrintHeader(writer);
            
            foreach (var v in elements.Values)
            {
                v.Print(printer, writer);
                writer.WriteLine();
            }

            printer.PrintFooter(writer);
        }
    }
}
