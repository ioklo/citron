namespace Citron;

using Citron.LexicalAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using static Citron.TextAnalysisTestMisc;

public class CommandLexerTests
{
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
        var context = MakeLexerContext("  p$$s${ ccc } \"ddd $e  \r\n }");

        var tokens = new List<Token>();

        context = RepeatLexCommand(tokens, lexer, context, 2);
        context = RepeatLexNormal(tokens, lexer, context, false, 2);
        context = RepeatLexCommand(tokens, lexer, context, 6);
            
        var expectedTokens = new Token[]
        {
            new TextToken("  p$s"),
            Tokens.DollarLBrace,
            new IdentifierToken("ccc"),
            Tokens.RBrace,
            new TextToken(" \"ddd "),
            new IdentifierToken("e"),
            new TextToken("  "),
            Tokens.NewLine,
            new TextToken(" "),
            Tokens.RBrace,
        };

        Assert.Equal(expectedTokens, tokens, TokenEqualityComparer.Instance);
    }

    [Fact]
    public void TestCommandModeLexCommands()
    {
        var lexer = new Lexer();
        var context = MakeLexerContext("ls -al");

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
        var context = MakeLexerContext(@"
hello world \n

    hello    

}");
        var tokens = new List<Token>();
        RepeatLexCommand(tokens, lexer, context, 6);

        var expected = new List<Token>
        {
            Tokens.NewLine,
            new TextToken("hello world \\n"), Tokens.NewLine, // skip multi newlines
            new TextToken("    hello    "), Tokens.NewLine,                
            Tokens.RBrace
        };

        Assert.Equal(expected, tokens, TokenEqualityComparer.Instance);
    }

    [Fact]
    public void TestCommandModeLexCommandsWithLineSeparator()
    {
        var lexer = new Lexer();
        var context = MakeLexerContext("ls -al\r\nbb");

        var result = Process(lexer, context);

        var expectedTokens = new Token[]
        {
            new TextToken("ls -al"),
            Tokens.NewLine,
            new TextToken("bb"),
        };

        Assert.Equal(expectedTokens, result, TokenEqualityComparer.Instance);
    }
}
