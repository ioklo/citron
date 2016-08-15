using Gum.Compiler;
using Gum.Data.AbstractSyntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gum.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            string code = File.ReadAllText(@"..\..\Tests\00_Print.gum");

            // TODO: preprocessing -> lexer에서 집어넣고 무시시켜야 할 듯
            code = Regex.Replace(code, "//.*$", " ", RegexOptions.Multiline);

            var lexer = new Lexer(code);
            var ast = Parser.ParseFileUnit(lexer);
        }
    }
}
