using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using Gum.Data;
using Gum.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Gum.LexerTest
{
    [TestClass]
    public class LexerTest
    {
        class TestCase
        {
            public string TestName { get; private set; }
            public string Text { get; private set; }
            public Token[] Results { get; private set; }
        }

        [TestMethod]
        public void Test()
        {
            var deserializer = new Deserializer();
            deserializer.RegisterTagMapping("!Token", typeof(Token));
            deserializer.RegisterTagMapping("!TokenType", typeof(TokenType));

            // 실행 파일 위치 기준으로..
            using (var reader = new StreamReader(@"..\..\Src\Test\LexerTest\LexerTest.yaml", Encoding.UTF8))
            {
                var testCases = deserializer.Deserialize<List<TestCase>>(reader);

                foreach (var testCase in testCases)
                {
                    TestSingle(testCase);
                }
            }
        }

        private void TestSingle(TestCase testCase)
        {
            Console.WriteLine(string.Format("Test \"{0}\"", testCase.TestName));

            var lexer = new Lexer(testCase.Text);
            Assert.IsFalse(lexer.End);

            var tokens = new List<Token>();

            int t = 0;
            do
            {
                Assert.AreEqual(testCase.Results[t].Type, lexer.TokenType);
                Assert.AreEqual(testCase.Results[t].Value, lexer.TokenValue);
                t++;

            } while (lexer.NextToken());

            Assert.AreEqual(testCase.Results.Length, t);
        }
    }
}
