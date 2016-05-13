using Gum.Prerequisite.Generator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Prerequisite
{
    class Program
    {
        static public void GenerateCSharpFile(string ns, string path, Structure structure)
        {
            var printer = new CSharpPrinter(ns);
            var stringWriter = new StringWriter();
            structure.Print(printer, stringWriter);
            var target = stringWriter.ToString();

            var fi = new FileInfo(path);
            if (fi.Exists)
            {
                string source = File.ReadAllText(fi.FullName);

                if (source != target)
                {
                    using (var streamWriter = new StreamWriter(fi.FullName))
                        streamWriter.Write(target);
                }
            }
            else
            {

                Directory.CreateDirectory(fi.DirectoryName);
                using (var streamWriter = new StreamWriter(fi.FullName))
                    streamWriter.Write(target);
            }
        }

        public static void Main(string[] args)
        {
            GenerateCSharpFile("Gum.Lang.AbstractSyntax", @"..\..\Src\Sandbox\Lang\AbstractSyntax\Generated.cs", AbstractSyntaxGenerator.Generate());
            GenerateCSharpFile("Gum.Lang.CoreIL", @"..\..\Src\Sandbox\Lang\CoreIL\Generated.cs", CoreILGenerator.Generate());
        }
    }
}
