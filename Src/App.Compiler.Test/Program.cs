using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Compiler compiler = new Compiler();
            Core.IL.Program prog = compiler.Compile("int a = 0;");

        }
    }
}
