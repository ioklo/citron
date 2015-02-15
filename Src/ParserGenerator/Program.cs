using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParserGenerator.AST;

namespace ParserGenerator
{
    class Program
    {
        // Const - Exp

        static void Main(string[] args)
        {
            int a = -3 - -4* -7 / -8;

            Dictionary<string, Tuple<bool, string>> TokenDecl = new Dictionary<string, Tuple<bool, string>>();

            using (var stream = new StreamReader(@"E:\Proj\ParserGenerator\parser.txt", Encoding.Default))
            {
                string code = stream.ReadToEnd();
                Parser parser = new Parser(code);

                ModuleNode node;

                if (!parser.ParseModule(out node)) return;
             
            }

            // NAME = "" | r""
            // 
        }
    }
}
