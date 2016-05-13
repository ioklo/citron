using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Prerequisite.Generator
{
    class CSharpPrinter : IElementPrinter
    {
        private string namespaceName;

        public CSharpPrinter(string namespaceName)
        {
            this.namespaceName = namespaceName;
        }

        string VarName(string name)
        {
            var sb = new StringBuilder(name);

            for (int t = 0; t < sb.Length; t++)
            {
                if (char.IsLower(sb[t])) break;
                sb[t] = char.ToLower(sb[t]);
            }

            return sb.ToString();
        }

        public void PrintHeader(TextWriter writer)
        {
            writer.WriteLine("using System;");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine("using System.IO;");
            writer.WriteLine("using System.Linq;");
            writer.WriteLine("using System.Text;");
            writer.WriteLine("using System.Threading.Tasks;");
            writer.WriteLine();

            writer.WriteLine("namespace {0}", namespaceName);
            writer.WriteLine("{");
        }

        public void PrintFooter(TextWriter writer)
        {
            writer.WriteLine("}");
        }

        public void Print(StructElem structElem, TextWriter Writer)
        {
            Writer.Write("    public class {0}", structElem.Name);
            if (structElem.Comps.Count != 0)
                Writer.Write(" : {0}", string.Join(", ", structElem.Comps.Select(comp => comp.Name)));
            Writer.WriteLine();

            Writer.WriteLine("    {");

            // properties
            foreach (var variable in structElem.Variables)
            {
                if (variable.VarType == VarType.Single)
                {
                    if( variable.Nullable )
                        Writer.WriteLine("        public {0}? {1} {{ get; private set; }}", variable.Type.Name, variable.Name);
                    else
                        Writer.WriteLine("        public {0} {1} {{ get; private set; }}", variable.Type.Name, variable.Name);
                }
                else if (variable.VarType == VarType.List)
                    Writer.WriteLine("        public IEnumerable<{0}> {1} {{ get; private set; }}", variable.Type.Name, variable.Name);
            }

            Writer.WriteLine();

            // default constructor
            Writer.WriteLine("        public {0}() {{ }}", structElem.Name);

            // constructor            
            if (0 < structElem.Variables.Count)
            {
                Writer.WriteLine("        public {0}({1})",
                    structElem.Name,
                    string.Join(", ", structElem.Variables.Select(var =>
                        {
                            if (var.VarType == VarType.Single)
                            {
                                if (var.Nullable)
                                    return string.Format("{0}? {1}", var.Type.Name, VarName(var.Name));
                                else
                                    return string.Format("{0} {1}", var.Type.Name, VarName(var.Name));
                            }
                            else if (var.VarType == VarType.List)
                                return string.Format("IEnumerable<{0}> {1}", var.Type.Name, VarName(var.Name));

                            return "";
                        }))
                    );
                Writer.WriteLine("        {");

                foreach (var variable in structElem.Variables)
                {
                    if (variable.VarType == VarType.List)
                        Writer.WriteLine("            this.{0} = {1}.ToList();", variable.Name, VarName(variable.Name));
                    else if (variable.VarType == VarType.Single)
                        Writer.WriteLine("            this.{0} = {1};", variable.Name, VarName(variable.Name));
                }

                Writer.WriteLine("        }");
            }

            // IFileUnit.Add(IStmtComponent)
            // ForStmt : IStmtComponent
            // comp의 Comps까지..
            foreach (var comp in structElem.AllComps)
            {
                Writer.WriteLine();
                Writer.WriteLine("        public void Visit({0}Visitor visitor)", comp.Name);
                Writer.WriteLine("        {");
                Writer.WriteLine("            visitor.Visit(this);");
                Writer.WriteLine("        }");
                Writer.WriteLine();
                Writer.WriteLine("        public void Visit<Arg0>({0}Visitor<Arg0> visitor, Arg0 arg0)", comp.Name);
                Writer.WriteLine("        {");
                Writer.WriteLine("            visitor.Visit(this, arg0);");
                Writer.WriteLine("        }");
            }

            Writer.WriteLine("    }");
        }

        public void Print(EnumElem enumElem, TextWriter Writer)
        {
            Writer.WriteLine("    public enum {0}", enumElem.Name);
            Writer.WriteLine("    {");
            foreach (var entry in enumElem.Entries)
            {
                Writer.WriteLine("        {0}, ", entry);
            }
            Writer.WriteLine("    }");
        }

        public void Print(ComponentElem compElem, TextWriter Writer)
        {
            Writer.Write("    public interface {0}", compElem.Name);
            if (compElem.Comps.Count != 0)
                Writer.Write(" : {0}", string.Join(", ", compElem.Comps.Select(comp => comp.Name)));


            Writer.WriteLine("    {");

            // ComponentVariables
            foreach(var variable in compElem.Variables)
            {
                Writer.WriteLine("        {0} {1} {{ get; }}", variable.Type.Name, variable.Name);
            }
            
            Writer.WriteLine("        void Visit({0}Visitor visitor);", compElem.Name);
            Writer.WriteLine("        void Visit<Arg0>({0}Visitor<Arg0> visitor, Arg0 arg0);", compElem.Name);
            Writer.WriteLine("    }");

            Writer.WriteLine();

            Writer.WriteLine("    public interface {0}Visitor", compElem.Name);
            Writer.WriteLine("    {");

            foreach (var elem in compElem.Elements)
            {
                Writer.WriteLine("        void Visit({0} {1});", elem.Name, VarName(elem.Name));
            }

            Writer.WriteLine("    }");

            Writer.WriteLine();

            Writer.WriteLine("    public interface {0}Visitor<Arg0>", compElem.Name);
            Writer.WriteLine("    {");

            foreach (var elem in compElem.Elements)
            {
                Writer.WriteLine("        void Visit({0} {1}, Arg0 arg0);", elem.Name, VarName(elem.Name));
            }

            Writer.WriteLine("    }");

        }

        public void Print(PrimitiveElem primitiveELem, TextWriter Writer)
        {

        }
    }
}
