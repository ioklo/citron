using System;
using Gum.App.Compiler;
using Gum.App.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gum.Test.Parser
{
    using Parser = Gum.App.Compiler.Parser;

    [TestClass]
    public class ParserTest
    {
        private void Test(string input, Action<Lexer, Parser> func)
        {
            Lexer lexer = new Lexer(input);
            Parser parser = new Parser();

            lexer.NextToken();
            func(lexer, parser);
        }

        [TestMethod]
        public void ParseIntegerTest()
        {
            Test("78", (lexer, parser) =>
            {
                IntegerExp ie;
                parser.ParseInteger(lexer, out ie);
                Assert.AreEqual(ie.Value, 78);
            });
        }

        [TestMethod]
        public void ParseBinaryOperationTest()
        {

            var data = new [] 
            {
                new { str = "==", kind = BinaryExpKind.Equal },
                new { str = "!=", kind = BinaryExpKind.NotEqual },
                new { str = "&&", kind = BinaryExpKind.And },
                new { str = "||", kind = BinaryExpKind.Or },                
                new { str = "+", kind = BinaryExpKind.Add },
                new { str = "-", kind = BinaryExpKind.Sub },
                new { str = "*", kind = BinaryExpKind.Mul},
                new { str = "/", kind = BinaryExpKind.Div },
                new { str = "<", kind = BinaryExpKind.Less},
                new { str = ">", kind = BinaryExpKind.Greater },
                new { str = "<=", kind = BinaryExpKind.LessEqual},
                new { str = ">=", kind = BinaryExpKind.GreaterEqual},
            };

            foreach (var entry in data)
            {
                Test(entry.str, (lexer, parser) =>
                {
                    BinaryExpKind kind;
                    parser.ParseBinaryOperation(lexer, out kind);

                    Assert.AreEqual(kind, entry.kind);
                });                
            }
        }


        [TestMethod]
        public void ParseExpTest()
        {
            Test("78 + 4", (lexer, parser) =>
            {
                IExp exp;
                parser.ParseExp(lexer, out exp);

                Assert.IsTrue(exp is BinaryExp);

                BinaryExp bexp = exp as BinaryExp;
                Assert.IsTrue(bexp.Operand1 is IntegerExp && ((IntegerExp)bexp.Operand1).Value == 78);
                Assert.IsTrue(bexp.Operand2 is IntegerExp && ((IntegerExp)bexp.Operand2).Value == 4);
                Assert.IsTrue(bexp.Operation == BinaryExpKind.Add);                
            });
        }

        
        [TestMethod]
        public void ParserExpTest1()
        {
            Parser parser = new Parser();
            Program pgm;

            bool res = parser.ParseProgram("bool a = (7 + 8 * 9 - (2 + 4) * 8 == 6);", out pgm);

            Assert.AreEqual(res, true);
        }

        [TestMethod]
        public void ParserTest2()
        {
            Parser parser = new Parser();
            Program pgm;

            bool res = parser.ParseProgram(

@"
int main(int g, int f)
{
    int t = 0;
    missingcall(2, 3, 4);

    while(true) { 2 + 3; }
    do {} while(false);

    return 0;
}
", out pgm);

            Assert.AreEqual(res, true);
        }
    }
}
