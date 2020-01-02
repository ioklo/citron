using Gum.LexicalAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Gum
{   
    public class CommandLexerTests
    {
        async ValueTask<LexerContext> MakeContextAsync(string text)
        {
            var buffer = new Buffer(new StringReader(text));
            return LexerContext.Make(await buffer.MakePosition().NextAsync());
        }

        async ValueTask<IEnumerable<Token>> ProcessAsync(Lexer lexer, LexerContext context)
        {
            var result = new List<Token>();

            while(!context.Pos.IsReachEnd())
            {
                var lexResult = await lexer.LexCommandModeAsync(context);
                if (!lexResult.HasValue) break;

                context = lexResult.Context;
                result.Add(lexResult.Token);
            }

            return result;
        }

        async ValueTask<LexerContext> RepeatLexNormalAsync(List<Token> tokens, Lexer lexer, LexerContext context, bool bSkipNewLine, int repeatCount)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                var result = await lexer.LexNormalModeAsync(context, bSkipNewLine);
                tokens.Add(result.Token); // ps
                context = result.Context;
            }

            return context;
        }

        async ValueTask<LexerContext> RepeatLexCommandAsync(List<Token> tokens, Lexer lexer, LexerContext context, int repeatCount)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                var result = await lexer.LexCommandModeAsync(context);
                tokens.Add(result.Token); // ps
                context = result.Context;
            }

            return context;
        }
            

        [Fact]
        public async Task TestLexerProcessStringExpInCommandMode()
        {
            var lexer = new Lexer();
            var context = await MakeContextAsync("  p$$s${ ccc } \"ddd $e  \r\n }");

            var tokens = new List<Token>();

            context = await RepeatLexCommandAsync(tokens, lexer, context, 2);
            context = await RepeatLexNormalAsync(tokens, lexer, context, false, 2);
            context = await RepeatLexCommandAsync(tokens, lexer, context, 6);
            
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

            Assert.Equal(expectedTokens, tokens);
        }

        [Fact]
        public async Task TestCommandModeLexCommandsAsync()
        {
            var lexer = new Lexer();
            var context = await MakeContextAsync("ls -al");

            var result = await ProcessAsync(lexer, context);

            var expectedTokens = new Token[]
            {
                new TextToken("ls -al")
            };

            Assert.Equal(expectedTokens, result);
        }

        [Fact]
        public async Task TestLexMultilinesAsync()
        {
            var lexer = new Lexer();
            var context = await MakeContextAsync(@"
hello world \n

    hello    

}");
            var tokens = new List<Token>();
            await RepeatLexCommandAsync(tokens, lexer, context, 6);

            var expected = new List<Token>
            {
                NewLineToken.Instance,
                new TextToken("hello world \\n"), NewLineToken.Instance, // skip multi newlines
                new TextToken("    hello    "), NewLineToken.Instance,                
                RBraceToken.Instance
            };

            Assert.Equal(expected, tokens);
        }

        [Fact]
        public async Task TestCommandModeLexCommandsWithLineSeparatorAsync()
        {
            var lexer = new Lexer();
            var context = await MakeContextAsync("ls -al\r\nbb");

            var result = await ProcessAsync(lexer, context);

            var expectedTokens = new Token[]
            {
                new TextToken("ls -al"),
                NewLineToken.Instance,
                new TextToken("bb"),
            };

            Assert.Equal(expectedTokens, result);
        }
    }
}
