namespace Citron;

using Citron.LexicalAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

using static Citron.TextAnalysisTestMisc;

public class LexerTests
{    
    IEnumerable<Token> ProcessInner(Func<LexerContext, LexResult> lexAction, LexerContext context)
    {
        var result = new List<Token>();

        while (true)
        {
            var lexResult = lexAction.Invoke(context);
            if (!lexResult.HasValue || lexResult.Token == Tokens.EndOfFile) break;

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
        var context = MakeLexerContext(
            "if else for continue break task params return async await foreach in yield seq enum struct class is as ref box null public protected private static " + 
            "new namespace " +
            "++ -- <= >= => == != ->" +
            "@ < > ; , = { } ( ) [ ] + - * / % ! . ? & : `");

        var tokens = ProcessNormal(lexer, context);
        var expectedTokens = new Token[]
        {
            Tokens.If,
            Tokens.Else,
            Tokens.For,
            Tokens.Continue,
            Tokens.Break,
            Tokens.Task,
            Tokens.Params,
            Tokens.Return,
            Tokens.Async,
            Tokens.Await,
            Tokens.Foreach,
            Tokens.In,
            Tokens.Yield,
            Tokens.Seq,
            Tokens.Enum,
            Tokens.Struct,
            Tokens.Class,
            Tokens.Is,
            Tokens.As,
            Tokens.Ref,
            Tokens.Box,
            Tokens.Null,

            Tokens.Public,
            Tokens.Protected,
            Tokens.Private,
            Tokens.Static,

            Tokens.New,
            Tokens.Namespace,

            Tokens.PlusPlus,
            Tokens.MinusMinus,
            Tokens.LessThanEqual,
            Tokens.GreaterThanEqual,
            Tokens.EqualGreaterThan,
            Tokens.EqualEqual,
            Tokens.ExclEqual,
            Tokens.MinusGreaterThan,

            Tokens.At,
            Tokens.LessThan,
            Tokens.GreaterThan,
            Tokens.SemiColon,
            Tokens.Comma,
            Tokens.Equal,
            Tokens.LBrace,
            Tokens.RBrace,
            Tokens.LParen,
            Tokens.RParen,
            Tokens.LBracket,
            Tokens.RBracket,

            Tokens.Plus,
            Tokens.Minus,
            Tokens.Star,
            Tokens.Slash,
            Tokens.Percent,
            Tokens.Excl,
            Tokens.Dot,
            Tokens.Question,
            Tokens.Ampersand,

            Tokens.Colon,
            Tokens.Backtick,
        };

        Assert.Equal(expectedTokens, tokens, TokenEqualityComparer.Instance);
    }

    [Fact]
    public void TestLexKeywords()
    {
        var lexer = new Lexer();
        var context = MakeLexerContext("true false");

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
        var token = lexer.LexNormalMode(MakeLexerContext("x"), false);

        Assert.True(token.HasValue);
        Assert.Equal(new IdentifierToken("x"), token.Token, TokenEqualityComparer.Instance);
    }

    [Fact]
    public void TestLexNormalString()
    {
        var context = MakeLexerContext("  \"aaa bbb \"  ");
        var lexer = new Lexer();
        var result0 = lexer.LexNormalMode(context, false);
        var result1 = lexer.LexStringMode(result0.Context);
        var result2 = lexer.LexStringMode(result1.Context);

        Assert.Equal(Tokens.DoubleQuote, result0.Token, TokenEqualityComparer.Instance);
        Assert.Equal(new TextToken("aaa bbb "), result1.Token, TokenEqualityComparer.Instance);
        Assert.Equal(Tokens.DoubleQuote, result2.Token, TokenEqualityComparer.Instance);
    }

    // stringMode
    [Fact]
    public void TestLexDoubleQuoteString()
    {
        var lexer = new Lexer();
        var context = MakeLexerContext("\"\"");

        var tokenResult = lexer.LexStringMode(context);

        var expectedToken = new TextToken("\"");

        Assert.Equal(expectedToken, tokenResult.Token, TokenEqualityComparer.Instance);
    }

    [Fact]
    public void TestLexDollarString()
    {
        var lexer = new Lexer();
        var context = MakeLexerContext("$$");

        var tokenResult = lexer.LexStringMode(context);
        var expectedToken = new TextToken("$");
        Assert.Equal(expectedToken, tokenResult.Token, TokenEqualityComparer.Instance);
    }

    [Fact]
    public void TestLexSimpleEscapedString2()
    {
        var lexer = new Lexer();
        var context = MakeLexerContext("$ccc");

        var tokenResult = lexer.LexStringMode(context);
        var expectedToken = new IdentifierToken("ccc");
        Assert.Equal(expectedToken, tokenResult.Token, TokenEqualityComparer.Instance);
    }

    [Fact]
    public void TestLexSimpleEscapedString()
    {
        var lexer = new Lexer();
        var context = MakeLexerContext("aaa bbb $ccc ddd");

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
        var context = MakeLexerContext("aaa bbb ${ccc} ddd"); // TODO: "aaa bbb ${ ccc \r\n } ddd" 는 에러

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
            Tokens.DollarLBrace,
            new IdentifierToken("ccc"),
            Tokens.RBrace,
            new TextToken(" ddd"),
        };

        Assert.Equal(expectedTokens, tokens, TokenEqualityComparer.Instance);
    }

    [Fact]
    public void TestLexComplexString()
    {
        var lexer = new Lexer();
        var context = MakeLexerContext("\"aaa bbb ${\"xxx ${ddd}\"} ddd\"");

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
            Tokens.DoubleQuote,
            new TextToken("aaa bbb "),
            Tokens.DollarLBrace,

            Tokens.DoubleQuote,

            new TextToken("xxx "),
            Tokens.DollarLBrace,
            new IdentifierToken("ddd"),
            Tokens.RBrace,
            Tokens.DoubleQuote,
            Tokens.RBrace,
            new TextToken(" ddd"),
            Tokens.DoubleQuote,
        };

        Assert.Equal(expectedTokens, tokens, TokenEqualityComparer.Instance);
    }

    [Fact]
    public void TestLexInt()
    {
        var lexer = new Lexer();
        var context = MakeLexerContext("1234"); // 나머지는 지원 안함

        var result = lexer.LexNormalMode(context, false);
        var expectedToken = new IntToken(1234);

        Assert.Equal(expectedToken, result.Token, TokenEqualityComparer.Instance);
    }

    [Fact]
    public void TestLexComment()
    {
        var lexer = new Lexer();
        var context = MakeLexerContext("  // e s \r\n// \r// \n1234"); // 나머지는 지원 안함

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
            Tokens.Whitespace,
            Tokens.NewLine,
            Tokens.Whitespace,
            Tokens.NewLine,
            Tokens.Whitespace,
            Tokens.NewLine,
            new IntToken(1234)
        };

        Assert.Equal(expectedTokens, tokens, TokenEqualityComparer.Instance);
    }

    [Fact]
    public void TestLexNextLine()
    {
        var lexer = new Lexer();
        var context = MakeLexerContext("1234 \\ // comment \r\n 55"); // 나머지는 지원 안함

        var tokens = new List<Token>();

        var result = lexer.LexInt(context);
        tokens.Add(result.Token);

        result = lexer.LexWhitespace(result.Context, false);
        tokens.Add(result.Token);

        result = lexer.LexInt(result.Context);
        tokens.Add(result.Token);

        var expectedTokens = new Token[] {
            new IntToken(1234),
            Tokens.Whitespace,                
            new IntToken(55)
        };

        Assert.Equal(expectedTokens, tokens, TokenEqualityComparer.Instance);
    }
}
