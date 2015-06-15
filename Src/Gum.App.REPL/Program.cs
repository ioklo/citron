using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.REPL
{
    class Program
    {
        // 환경: 변수들, 타입들 
        static void Main(string[] args)
        {
            Console.WriteLine("| Type '#exit' to exit.");

            while(true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();

                if( line.Trim().Equals("#exit") ) break;
            }            
        }
    }
}
