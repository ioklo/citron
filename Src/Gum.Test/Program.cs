using Gum.Core.IL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Gum.App.Compiler.Parser parser = new Gum.App.Compiler.Parser();
            string code =
@"
int a = 7;
string g = ""aaaa"";

void WriteLine(int val);

int main()
{
    int b = a;

    WriteLine(b);
    return 0;
}
";
            Gum.Core.IL.Domain env = new Domain();
            var compiledPgm = Gum.App.Compiler.Compiler.Compile(env, code);

            // 컴파일러는 프로그램을 만들고 VM 인터프리터는 그것을 실행한다
            var vm = new Gum.App.VM.Interpreter();

            // WriteLine이란 함수는 외부함수..
            vm.AddExternFunc("WriteLine", WriteLine);

            // main 부분을 실행한다
            vm.Call(env, "main");          

        }

        public static object WriteLine(object[] ps)
        {
            Console.WriteLine(ps[0]);
            return null;
        }
    }

}
