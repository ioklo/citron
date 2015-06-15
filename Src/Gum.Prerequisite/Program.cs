using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Prerequisite
{
    class Program
    {
        public static void Main(string[] args)
        {
            GenerateAbstractSyntax.GenerateCSharpFile();
            GenerateTypedAbstractSyntax.GenerateCSharpFile();
            GenerateCoreIL.GenerateCSharpFile();
        }
    }
}
