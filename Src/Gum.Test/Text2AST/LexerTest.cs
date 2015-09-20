using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using Gum.Translator.Text2AST;

namespace Gum.Test.Text2AST
{
    class LexerTestCase : ITestCase
    {
        public string TestName { get; private set; }
        public string Text { get; private set; }
        public Token[] Results { get; private set; }
    }

    class LexerTest : TestBase<LexerTestCase>
    {
        public override void ConfigDeserializer(Deserializer deserializer)
        {
            deserializer.RegisterTagMapping("!Token", typeof(Token));
            deserializer.RegisterTagMapping("!TokenType", typeof(TokenType));
        }

        public override bool Test(LexerTestCase testCase)
        {
            var lexer = new Lexer(testCase.Text);

            var tokens = new List<Token>();
            while (lexer.NextToken())
                tokens.Add(new Token(lexer.TokenType, lexer.TokenValue));

            if (testCase.Results.Length != tokens.Count)
                return false;

            for (int t = 0; t < testCase.Results.Length; t++)
                if (testCase.Results[t].Type != tokens[t].Type ||
                    testCase.Results[t].Value != tokens[t].Value)
                    return false;

            return true;
        }
    }
}
