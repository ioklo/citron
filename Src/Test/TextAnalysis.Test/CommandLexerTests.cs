using Citron.LexicalAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Citron.TextAnalysis.Test
{   
    public class CommandLexerTests
    {
        LexerContext MakeContext(string text)
        {
            var buffer = new Buffer(new StringReader(text));
            return LexerContext.Make(buffer.MakePosition().Next());
        }

        IEnumerable<Token> Process(Lexer lexer, LexerContext context)
        {
            var result = new List<Token>();

            while(!context.Pos.IsReachEnd())
            {
                var lexResult = lexer.LexCommandMode(context);
                if (!lexResult.HasValue) break;

                context = lexResult.Context;
                result.Add(lexResult.Token);
            }

            return result;
        }

        LexerContext RepeatLexNormal(List<Token> tokens, Lexer lexer, LexerContext context, bool bSkipNewLine, int repeatCount)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                var result = lexer.LexNormalMode(context, bSkipNewLine);
                tokens.Add(result.Token); // ps
                context = result.Context;
            }

            return context;
        }

        LexerContext RepeatLexCommand(List<Token> tokens, Lexer lexer, LexerContext context, int repeatCount)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                var result = lexer.LexCommandMode(context);
                tokens.Add(result.Token); // ps
                context = result.Context;
            }

            return context;
        }
            

        [Fact]
        public void TestLexerProcessStringExpInCommandMode()
        {
            var lexer = new Lexer();
            var context = MakeContext("  p$$s${ ccc } \"ddd $e  \r\n }");

            var tokens = new List<Token>();

            context = RepeatLexCommand(tokens, lexer, context, 2);
            context = RepeatLexNormal(tokens, lexer, context, false, 2);
            context = RepeatLexCommand(tokens, lexer, context, 6);
            
            var expectedTokens = new Token[]
            {
                new TextToken("  p$s"),
                DollarLBraceToken.Instance,
                new IdentifierToken("ccc"),
                RBraceToken.Instance,
                new TextToken(" \"ddd "),
                new IdentifierToken("e"),
                new TextToken("  "),
                NewLineToken.Instance,
                new TextToken(" "),
                RBraceToken.Instance,
            };

            Assert.Equal(expectedTokens, tokens, TokenEqualityComparer.Instance);
        }

        [Fact]
        public void TestCommandModeLexCommands()
        {
            var lexer = new Lexer();
            var context = MakeContext("ls -al");

            var result = Process(lexer, context);

            var expectedTokens = new Token[]
            {
                new TextToken("ls -al")
            };

            Assert.Equal(expectedTokens, result, TokenEqualityComparer.Instance);
        }

        [Fact]
        public void TestLexMultilines()
        {
            var lexer = new Lexer();
            var context = MakeContext(@"
hello world \n

    hello    

}");
            var tokens = new List<Token>();
            RepeatLexCommand(tokens, lexer, context, 6);

            var expected = new List<Token>
            {
                NewLineToken.Instance,
                new TextToken("hello world \\n"), NewLineToken.Instance, // skip multi newlines
                new TextToken("    hello    "), NewLineToken.Instance,                
                RBraceToken.Instance
            };

            Assert.Equal(expected, tokens, TokenEqualityComparer.Instance);
        }

        [Fact]
        public void TestCommandModeLexCommandsWithLineSeparator()
        {
            var lexer = new Lexer();
            var context = MakeContext("ls -al\r\nbb");

            var result = Process(lexer, context);

            var expectedTokens = new Token[]
            {
                new TextToken("ls -al"),
                NewLineToken.Instance,
                new TextToken("bb"),
            };

            Assert.Equal(expectedTokens, result, TokenEqualityComparer.Instance);
        }
    }
}
