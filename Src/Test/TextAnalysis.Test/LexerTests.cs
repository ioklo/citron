using Citron.LexicalAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Citron.TextAnalysis.Test
{
    public class LexerTests
    {
        LexerContext MakeContext(string text)
        {
            var buffer = new Buffer(new StringReader(text));
            return LexerContext.Make(buffer.MakePosition().Next()); // TODO: Position 관련 동작 재 수정
        }

        IEnumerable<Token> ProcessInner(Func<LexerContext, LexResult> lexAction, LexerContext context)
        {
            var result = new List<Token>();

            while (true)
            {
                var lexResult = lexAction.Invoke(context);
                if (!lexResult.HasValue || lexResult.Token is EndOfFileToken) break;

                context = lexResult.Context;

                result.Add(lexResult.Token);
            }

            return result;
        }

        IEnumerable<Token> ProcessNormal(Lexer lexer, LexerContext context)
        {
            return ProcessInner(context => lexer.LexNormalMode(context, false), context);
        }

        IEnumerable<Token> ProcessString(Lexer lexer, LexerContext context)
        {
            return ProcessInner(context => lexer.LexStringMode(context), context);
        }

        [Fact]
        public void TestLexSymbols()
        {
            var lexer = new Lexer();
            var context = MakeContext(
                "if else for continue break exec task params return async await foreach in yield seq enum struct class is ref box null public protected private static " + 
                "new namespace " +
                "++ -- <= >= => == != " +
                "@ < > ; , = { } ( ) [ ] + - * / % ! . ? & : `");

            var tokens = ProcessNormal(lexer, context);
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
                ClassToken.Instance,
                IsToken.Instance,
                RefToken.Instance,
                BoxToken.Instance,
                NullToken.Instance,

                PublicToken.Instance,
                ProtectedToken.Instance,
                PrivateToken.Instance,
                StaticToken.Instance,

                NewToken.Instance,
                NamespaceToken.Instance,

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
                QuestionToken.Instance,
                AmpersandToken.Instance,

                ColonToken.Instance,
                BacktickToken.Instance,
            };

            Assert.Equal(expectedTokens, tokens, TokenEqualityComparer.Instance);
        }

        [Fact]
        public void TestLexKeywords()
        {
            var lexer = new Lexer();
            var context = MakeContext("true false");

            var tokens = ProcessNormal(lexer, context);
            var expectedTokens = new Token[]
            {
                new BoolToken(true),
                new BoolToken(false),
            };

            Assert.Equal(expectedTokens, tokens, TokenEqualityComparer.Instance);
        }

        [Fact]
        public void TestLexSimpleIdentifier()
        {
            var lexer = new Lexer();
            var token = lexer.LexNormalMode(MakeContext("x"), false);

            Assert.True(token.HasValue);
            Assert.Equal(new IdentifierToken("x"), token.Token, TokenEqualityComparer.Instance);
        }

        [Fact]
        public void TestLexNormalString()
        {
            var context = MakeContext("  \"aaa bbb \"  ");
            var lexer = new Lexer();
            var result0 = lexer.LexNormalMode(context, false);
            var result1 = lexer.LexStringMode(result0.Context);
            var result2 = lexer.LexStringMode(result1.Context);

            Assert.Equal(DoubleQuoteToken.Instance, result0.Token, TokenEqualityComparer.Instance);
            Assert.Equal(new TextToken("aaa bbb "), result1.Token, TokenEqualityComparer.Instance);
            Assert.Equal(DoubleQuoteToken.Instance, result2.Token, TokenEqualityComparer.Instance);
        }

        // stringMode
        [Fact]
        public void TestLexDoubleQuoteString()
        {
            var lexer = new Lexer();
            var context = MakeContext("\"\"");

            var tokenResult = lexer.LexStringMode(context);

            var expectedToken = new TextToken("\"");

            Assert.Equal(expectedToken, tokenResult.Token, TokenEqualityComparer.Instance);
        }

        [Fact]
        public void TestLexDollarString()
        {
            var lexer = new Lexer();
            var context = MakeContext("$$");

            var tokenResult = lexer.LexStringMode(context);
            var expectedToken = new TextToken("$");
            Assert.Equal(expectedToken, tokenResult.Token, TokenEqualityComparer.Instance);
        }

        [Fact]
        public void TestLexSimpleEscapedString2()
        {
            var lexer = new Lexer();
            var context = MakeContext("$ccc");

            var tokenResult = lexer.LexStringMode(context);
            var expectedToken = new IdentifierToken("ccc");
            Assert.Equal(expectedToken, tokenResult.Token, TokenEqualityComparer.Instance);
        }

        [Fact]
        public void TestLexSimpleEscapedString()
        {
            var lexer = new Lexer();
            var context = MakeContext("aaa bbb $ccc ddd");

            var tokens = ProcessString(lexer, context);

            var expectedTokens = new Token[]
            {
                new TextToken("aaa bbb "),
                new IdentifierToken("ccc"),
                new TextToken(" ddd"),
            };

            Assert.Equal(expectedTokens, tokens, TokenEqualityComparer.Instance);
        }

        [Fact]
        public void TestLexEscapedString()
        {
            var lexer = new Lexer();
            var context = MakeContext("aaa bbb ${ccc} ddd"); // TODO: "aaa bbb ${ ccc \r\n } ddd" 는 에러

            var tokens = new List<Token>();
            var result = lexer.LexStringMode(context);
            tokens.Add(result.Token);

            result = lexer.LexStringMode(result.Context);
            tokens.Add(result.Token);

            result = lexer.LexNormalMode(result.Context, false);
            tokens.Add(result.Token);

            result = lexer.LexNormalMode(result.Context, false);
            tokens.Add(result.Token);

            result = lexer.LexStringMode(result.Context);
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
        public void TestLexComplexString()
        {
            var lexer = new Lexer();
            var context = MakeContext("\"aaa bbb ${\"xxx ${ddd}\"} ddd\"");

            var tokens = new List<Token>();
            var result = lexer.LexNormalMode(context, false);
            tokens.Add(result.Token); // "

            result = lexer.LexStringMode(result.Context);
            tokens.Add(result.Token); // aaa bbb

            result = lexer.LexStringMode(result.Context);
            tokens.Add(result.Token); // ${

            result = lexer.LexNormalMode(result.Context, false);
            tokens.Add(result.Token); // "

            result = lexer.LexStringMode(result.Context);
            tokens.Add(result.Token); // xxx 

            result = lexer.LexStringMode(result.Context);
            tokens.Add(result.Token); // ${

            result = lexer.LexNormalMode(result.Context, false);
            tokens.Add(result.Token); // ddd

            result = lexer.LexNormalMode(result.Context, false);
            tokens.Add(result.Token); // }

            result = lexer.LexStringMode(result.Context);
            tokens.Add(result.Token); // "

            result = lexer.LexNormalMode(result.Context, false);
            tokens.Add(result.Token); // }

            result = lexer.LexStringMode(result.Context);
            tokens.Add(result.Token); // ddd 

            result = lexer.LexStringMode(result.Context);
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
        public void TestLexInt()
        {
            var lexer = new Lexer();
            var context = MakeContext("1234"); // 나머지는 지원 안함

            var result = lexer.LexNormalMode(context, false);
            var expectedToken = new IntToken(1234);

            Assert.Equal(expectedToken, result.Token, TokenEqualityComparer.Instance);
        }

        [Fact]
        public void TestLexComment()
        {
            var lexer = new Lexer();
            var context = MakeContext("  // e s \r\n// \r// \n1234"); // 나머지는 지원 안함

            var tokens = new List<Token>();

            var result = lexer.LexWhitespace(context, false);
            tokens.Add(result.Token);

            result = lexer.LexNewLine(result.Context);
            tokens.Add(result.Token);

            result = lexer.LexWhitespace(result.Context, false);
            tokens.Add(result.Token);

            result = lexer.LexNewLine(result.Context);
            tokens.Add(result.Token);

            result = lexer.LexWhitespace(result.Context, false);
            tokens.Add(result.Token);

            result = lexer.LexNewLine(result.Context);
            tokens.Add(result.Token);

            result = lexer.LexInt(result.Context);
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
        public void TestLexNextLine()
        {
            var lexer = new Lexer();
            var context = MakeContext("1234 \\ // comment \r\n 55"); // 나머지는 지원 안함

            var tokens = new List<Token>();

            var result = lexer.LexInt(context);
            tokens.Add(result.Token);

            result = lexer.LexWhitespace(result.Context, false);
            tokens.Add(result.Token);

            result = lexer.LexInt(result.Context);
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
