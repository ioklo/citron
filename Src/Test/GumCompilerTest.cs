using System;
using Gum.App.Compiler;
using Gum.App.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gum.Test
{
    using IL = Gum.Core.IL;

    [TestClass]
    public class GumCompilerTest
    {
        [TestMethod]
        public void LexerTest()
        {
            Lexer lexer = new Lexer("hi \r\n 12300 int");

            var res = lexer.NextToken();

            Assert.AreEqual(lexer.Token, "hi");
            Assert.AreEqual(res, Lexer.TokenKind.Identifier);

            res = lexer.NextToken();

            
            Assert.AreEqual(lexer.Token, "12300");
            Assert.AreEqual(res, Lexer.TokenKind.IntValue);

            res = lexer.NextToken();
            
            Assert.AreEqual(lexer.Token, "int");
            Assert.AreEqual(res, Lexer.TokenKind.Identifier);

            res = lexer.NextToken();
            
            Assert.AreEqual(lexer.Token, string.Empty);
            Assert.AreEqual(res, Lexer.TokenKind.Invalid);

        }

        [TestMethod]
        public void ParserTest1()
        {
            Gum.App.Compiler.Parser parser = new Gum.App.Compiler.Parser();
            Program pgm;

            bool res = parser.ParseProgram("int a = 0; \r\n int b; string g = \"aaaa\";", out pgm);

            Assert.AreEqual(res, true);
            Assert.AreEqual(pgm.Decls.Count, 3);                    
        }

        // 함수..
        public object WriteLine(object[] ps)
        {
            Console.WriteLine(ps[0]);
            return null;
        }


        [TestMethod]
        public void  CompilerTest()
        {
            Gum.App.Compiler.Parser parser = new Gum.App.Compiler.Parser();
            string code =
@"
int a = 0;
string g = ""aaaa"";

void WriteLine(string val);

int main()
{
    int b = a;    

    WriteLine(g);
    return 0;
}
";            
            Gum.App.Compiler.Compiler compiler = new Gum.App.Compiler.Compiler();
            var compiledPgm = compiler.Compile(code);

            // 컴파일러는 프로그램을 만들고 VM 인터프리터는 그것을 실행한다
            var vm = new Gum.App.VM.Interpreter(compiledPgm);            

            // WriteLine이란 함수는 외부함수..
            vm.AddExternFunc("WriteLine", WriteLine);

            // main 부분을 실행한다
            vm.Call("main");          

        }

        
    }
}
