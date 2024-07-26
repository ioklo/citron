#include "pch.h"
#include <vector>

#include <TextAnalysis/Buffer.h>
#include <TextAnalysis/BufferPosition.h>
#include <TextAnalysis/Lexer.h>

#include "TestMisc.h"

using namespace std;
using namespace Citron;

void RepeatLexCommand(vector<Token>* tokens, Lexer* lexer, int repeatCount)
{
    for (int i = 0; i < repeatCount; i++)
    {
        auto optLexResult = lexer->LexCommandMode();
        ASSERT_TRUE(optLexResult);
        
        tokens->push_back(std::move(optLexResult->token)); // ps
        *lexer = std::move(optLexResult->lexer);
    }
}

void RepeatLexNormal(vector<Token>* tokens, Lexer* lexer, bool bSkipNewLine, int repeatCount)
{
    for (int i = 0; i < repeatCount; i++)
    {
        auto optLexResult = lexer->LexNormalMode(bSkipNewLine);
        ASSERT_TRUE(optLexResult);

        tokens->push_back(std::move(optLexResult->token)); // ps
        *lexer = std::move(optLexResult->lexer);
    }
}

vector<Token> Process(Lexer lexer)
{
    vector<Token> tokens;
    while (!lexer.IsReachedEnd())
    {
        auto optLexResult = lexer.LexCommandMode();
        if (!optLexResult) break;

        tokens.push_back(std::move(optLexResult->token));
        lexer = std::move(optLexResult->lexer);
    }

    return tokens;
}

TEST(CommandLexer, ProcessStringExpInCommandMode)
{   
    auto [buffer, lexer] = Prepare(U"  p$$s${ ccc } \"ddd $e  \r\n }"s);

    vector<Token> tokens;

    RepeatLexCommand(&tokens, &lexer, 2);
    RepeatLexNormal(&tokens, &lexer, false, 2);
    RepeatLexCommand(&tokens, &lexer, 6);

    vector<Token> expectedTokens = {
        TextToken("  p$s"),
        DollarLBraceToken(),
        IdentifierToken("ccc"),
        RBraceToken(),
        TextToken(" \"ddd "),
        IdentifierToken("e"),
        TextToken("  "),
        NewLineToken(),
        TextToken(" "),
        RBraceToken(),
    };

    EXPECT_EQ(tokens, expectedTokens);
}

TEST(CommandLexer, LexCommands)
{
    auto [buffer, lexer] = Prepare(U"ls -al");
    auto tokens = Process(lexer);

    vector<Token> expectedTokens = {
        TextToken("ls -al")
    };

    EXPECT_EQ(tokens, expectedTokens);
}

TEST(CommandLexer, LexMultiLines)
{
    auto [buffer, lexer] = Prepare(UR"---(
hello world \n

    hello    

})---"s);

    vector<Token> tokens;
    RepeatLexCommand(&tokens, &lexer, 6);

    vector<Token> expected = {
        NewLineToken(),
        TextToken("hello world \\n"), NewLineToken(), // skip multi newlines
        TextToken("    hello    "), NewLineToken(),
        RBraceToken()
    };

    EXPECT_EQ(tokens, expected);
}

TEST(CommandLexer, LexCommandsWithLineSeparator)
{
    auto [buffer, lexer] = Prepare(U"ls -al\r\nbb"s);
    auto tokens = Process(lexer);

    vector<Token> expected = {
        TextToken("ls -al"),
        NewLineToken(),
        TextToken("bb"),
    };

    EXPECT_EQ(tokens, expected);
}