using Gum.LexicalAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Gum.TextAnalysis.Test
{
    public class LexerTests
    {
        async ValueTask<LexerContext> MakeContextAsync(string text)
        {
            var buffer = new Buffer(new StringReader(text));
            return LexerContext.Make(await buffer.MakePosition().NextAsync()); // TODO: Position 관련 동작 재 수정
        }

        async ValueTask<IEnumerable<Token>> ProcessInnerAsync(Func<LexerContext, ValueTask<LexResult>> lexAction, LexerContext context)
        {
            var result = new List<Token>();

            while (true)
            {
                var lexResult = await lexAction(context);
                if (!lexResult.HasValue || lexResult.Token is EndOfFileToken) break;

                context = lexResult.Context;

                result.Add(lexResult.Token);
            }

            return result;
        }

        ValueTask<IEnumerable<Token>> ProcessNormalAsync(Lexer lexer, LexerContext context)
        {
            return ProcessInnerAsync(context => lexer.LexNormalModeAsync(context, false), context);
        }

        ValueTask<IEnumerable<Token>> ProcessStringAsync(Lexer lexer, LexerContext context)
        {
            return ProcessInnerAsync(context => lexer.LexStringModeAsync(context), context);
        }

        [Fact]
        public async Task TestLexSymbols()
        {
            var lexer = new Lexer();
            var context = await MakeContextAsync(
                "if else for continue break exec task params return async await foreach in yield seq enum struct is ref public protected private static " + 
                "new " +
                "++ -- <= >= => == != " +
                "@ < > ; , = { } ( ) [ ] + - * / % ! . : `");

            var tokens = await ProcessNormalAsync(lexer, context);
            var expectedTokens = new Token[]
            {
                IfToken.Instance,
                ElseToken.Instance,
                ForToken.Instance,
                ContinueToken.Instance,
                BreakToken.Instance,
                ExecToken.Instance,
                TaskToken.Instance,
                ParamsToken.Instance,
                ReturnToken.Instance,
                AsyncToken.Instance,
                AwaitToken.Instance,
                ForeachToken.Instance,
                InToken.Instance,
                YieldToken.Instance,
                SeqToken.Instance,
                EnumToken.Instance,
                StructToken.Instance,
                IsToken.Instance,
                RefToken.Instance,

                PublicToken.Instance,
                ProtectedToken.Instance,
                PrivateToken.Instance,
                StaticToken.Instance,

                NewToken.Instance,

                PlusPlusToken.Instance,
                MinusMinusToken.Instance,
                LessThanEqualToken.Instance,
                GreaterThanEqualToken.Instance,
                EqualGreaterThanToken.Instance,
                EqualEqualToken.Instance,
                ExclEqualToken.Instance,

                ExecToken.Instance,
                LessThanToken.Instance,
                GreaterThanToken.Instance,
                SemiColonToken.Instance,
                CommaToken.Instance,
                EqualToken.Instance,
                LBraceToken.Instance,
                RBraceToken.Instance,
                LParenToken.Instance,
                RParenToken.Instance,
                LBracketToken.Instance,
                RBracketToken.Instance,

                PlusToken.Instance,
                MinusToken.Instance,
                StarToken.Instance,
                SlashToken.Instance,
                PercentToken.Instance,
                ExclToken.Instance,
                DotToken.Instance,

                ColonToken.Instance,
                BacktickToken.Instance,
            };

            Assert.Equal(expectedTokens, tokens, TokenEqualityComparer.Instance);
        }

        [Fact]
        public async Task TestLexKeywords()
        {
            var lexer = new Lexer();
            var context = await MakeContextAsync("true false");

            var tokens = await ProcessNormalAsync(lexer, context);
            var expectedTokens = new Token[]
            {
                new BoolToken(true),
                new BoolToken(false),
            };

            Assert.Equal(expectedTokens, tokens, TokenEqualityComparer.Instance);
        }

        [Fact]
        public async Task TestLexSimpleIdentifier()
        {
            var lexer = new Lexer();
            var token = await lexer.LexNormalModeAsync(await MakeContextAsync("x"), false);

            Assert.True(token.HasValue);
            Assert.Equal(new IdentifierToken("x"), token.Token, TokenEqualityComparer.Instance);
        }

        [Fact]
        public async Task TestLexNormalString()
        {
            var context = await MakeContextAsync("  \"aaa bbb \"  ");
            var lexer = new Lexer();
            var result0 = await lexer.LexNormalModeAsync(context, false);
            var result1 = await lexer.LexStringModeAsync(result0.Context);
            var result2 = await lexer.LexStringModeAsync(result1.Context);

            Assert.Equal(DoubleQuoteToken.Instance, result0.Token, TokenEqualityComparer.Instance);
            Assert.Equal(new TextToken("aaa bbb "), result1.Token, TokenEqualityComparer.Instance);
            Assert.Equal(DoubleQuoteToken.Instance, result2.Token, TokenEqualityComparer.Instance);
        }

        // stringMode
        [Fact]
        public async Task TestLexDoubleQuoteString()
        {
            var lexer = new Lexer();
            var context = await MakeContextAsync("\"\"");

            var tokenResult = await lexer.LexStringModeAsync(context);

            var expectedToken = new TextToken("\"");

            Assert.Equal(expectedToken, tokenResult.Token, TokenEqualityComparer.Instance);
        }

        [Fact]
        public async Task TestLexDollarString()
        {
            var lexer = new Lexer();
            var context = await MakeContextAsync("$$");

            var tokenResult = await lexer.LexStringModeAsync(context);
            var expectedToken = new TextToken("$");
            Assert.Equal(expectedToken, tokenResult.Token, TokenEqualityComparer.Instance);
        }

        [Fact]
        public async Task TestLexSimpleEscapedString2()
        {
            var lexer = new Lexer();
            var context = await MakeContextAsync("$ccc");

            var tokenResult = await lexer.LexStringModeAsync(context);
            var expectedToken = new IdentifierToken("ccc");
            Assert.Equal(expectedToken, tokenResult.Token, TokenEqualityComparer.Instance);
        }

        [Fact]
        public async Task TestLexSimpleEscapedString()
        {
            var lexer = new Lexer();
            var context = await MakeContextAsync("aaa bbb $ccc ddd");

            var tokens = await ProcessStringAsync(lexer, context);

            var expectedTokens = new Token[]
            {
                new TextToken("aaa bbb "),
                new IdentifierToken("ccc"),
                new TextToken(" ddd"),
            };

            Assert.Equal(expectedTokens, tokens, TokenEqualityComparer.Instance);
        }

        [Fact]
        public async Task TestLexEscapedString()
        {
            var lexer = new Lexer();
            var context = await MakeContextAsync("aaa bbb ${ccc} ddd"); // TODO: "aaa bbb ${ ccc \r\n } ddd" 는 에러

            var tokens = new List<Token>();
            var result = await lexer.LexStringModeAsync(context);
            tokens.Add(result.Token);

            result = await lexer.LexStringModeAsync(result.Context);
            tokens.Add(result.Token);

            result = await lexer.LexNormalModeAsync(result.Context, false);
            tokens.Add(result.Token);

            result = await lexer.LexNormalModeAsync(result.Context, false);
            tokens.Add(result.Token);

            result = await lexer.LexStringModeAsync(result.Context);
            tokens.Add(result.Token);

            var expectedTokens = new Token[]
            {
                new TextToken("aaa bbb "),
                DollarLBraceToken.Instance,
                new IdentifierToken("ccc"),
                RBraceToken.Instance,
                new TextToken(" ddd"),
            };

            Assert.Equal(expectedTokens, tokens, TokenEqualityComparer.Instance);
        }

        [Fact]
        public async Task TestLexComplexString()
        {
            var lexer = new Lexer();
            var context = await MakeContextAsync("\"aaa bbb ${\"xxx ${ddd}\"} ddd\"");

            var tokens = new List<Token>();
            var result = await lexer.LexNormalModeAsync(context, false);
            tokens.Add(result.Token); // "

            result = await lexer.LexStringModeAsync(result.Context);
            tokens.Add(result.Token); // aaa bbb

            result = await lexer.LexStringModeAsync(result.Context);
            tokens.Add(result.Token); // ${

            result = await lexer.LexNormalModeAsync(result.Context, false);
            tokens.Add(result.Token); // "

            result = await lexer.LexStringModeAsync(result.Context);
            tokens.Add(result.Token); // xxx 

            result = await lexer.LexStringModeAsync(result.Context);
            tokens.Add(result.Token); // ${

            result = await lexer.LexNormalModeAsync(result.Context, false);
            tokens.Add(result.Token); // ddd

            result = await lexer.LexNormalModeAsync(result.Context, false);
            tokens.Add(result.Token); // }

            result = await lexer.LexStringModeAsync(result.Context);
            tokens.Add(result.Token); // "

            result = await lexer.LexNormalModeAsync(result.Context, false);
            tokens.Add(result.Token); // }

            result = await lexer.LexStringModeAsync(result.Context);
            tokens.Add(result.Token); // ddd 

            result = await lexer.LexStringModeAsync(result.Context);
            tokens.Add(result.Token); // "

            var expectedTokens = new Token[]
            {
                DoubleQuoteToken.Instance,
                new TextToken("aaa bbb "),
                DollarLBraceToken.Instance,

                DoubleQuoteToken.Instance,

                new TextToken("xxx "),
                DollarLBraceToken.Instance,
                new IdentifierToken("ddd"),
                RBraceToken.Instance,
                DoubleQuoteToken.Instance,
                RBraceToken.Instance,
                new TextToken(" ddd"),
                DoubleQuoteToken.Instance,
            };

            Assert.Equal(expectedTokens, tokens, TokenEqualityComparer.Instance);
        }

        [Fact]
        public async Task TestLexInt()
        {
            var lexer = new Lexer();
            var context = await MakeContextAsync("1234"); // 나머지는 지원 안함

            var result = await lexer.LexNormalModeAsync(context, false);
            var expectedToken = new IntToken(1234);

            Assert.Equal(expectedToken, result.Token, TokenEqualityComparer.Instance);
        }

        [Fact]
        public async Task TestLexComment()
        {
            var lexer = new Lexer();
            var context = await MakeContextAsync("  // e s \r\n// \r// \n1234"); // 나머지는 지원 안함

            var tokens = new List<Token>();

            var result = await lexer.LexWhitespaceAsync(context, false);
            tokens.Add(result.Token);

            result = await lexer.LexNewLineAsync(result.Context);
            tokens.Add(result.Token);

            result = await lexer.LexWhitespaceAsync(result.Context, false);
            tokens.Add(result.Token);

            result = await lexer.LexNewLineAsync(result.Context);
            tokens.Add(result.Token);

            result = await lexer.LexWhitespaceAsync(result.Context, false);
            tokens.Add(result.Token);

            result = await lexer.LexNewLineAsync(result.Context);
            tokens.Add(result.Token);

            result = await lexer.LexIntAsync(result.Context);
            tokens.Add(result.Token);

            var expectedTokens = new Token[] {
                WhitespaceToken.Instance,
                NewLineToken.Instance,
                WhitespaceToken.Instance,
                NewLineToken.Instance,
                WhitespaceToken.Instance,
                NewLineToken.Instance,
                new IntToken(1234)
            };

            Assert.Equal(expectedTokens, tokens, TokenEqualityComparer.Instance);
        }

        [Fact]
        public async Task TestLexNextLine()
        {
            var lexer = new Lexer();
            var context = await MakeContextAsync("1234 \\ // comment \r\n 55"); // 나머지는 지원 안함

            var tokens = new List<Token>();

            var result = await lexer.LexIntAsync(context);
            tokens.Add(result.Token);

            result = await lexer.LexWhitespaceAsync(result.Context, false);
            tokens.Add(result.Token);

            result = await lexer.LexIntAsync(result.Context);
            tokens.Add(result.Token);

            var expectedTokens = new Token[] {
                new IntToken(1234),
                WhitespaceToken.Instance,                
                new IntToken(55)
            };

            Assert.Equal(expectedTokens, tokens, TokenEqualityComparer.Instance);
        }
    }
}
