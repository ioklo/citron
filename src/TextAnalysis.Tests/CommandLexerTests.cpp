#include "pch.h"
#include <vector>

#include <TextAnalysis/Buffer.h>
#include <TextAnalysis/BufferPosition.h>
#include <TextAnalysis/Lexer.h>

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

std::tuple<shared_ptr<Buffer>, Lexer> Make(u32string str)
{
    auto buffer = make_shared<Buffer>(str);
    BufferPosition pos = buffer->MakeStartPosition();
    return { std::move(buffer), Lexer(pos) };
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
    auto buffer = make_shared<Buffer>(U"  p$$s${ ccc } \"ddd $e  \r\n }");
    BufferPosition pos = buffer->MakeStartPosition();
    Lexer lexer(pos);

    vector<Token> tokens;

    RepeatLexCommand(&tokens, &lexer, 2);
    RepeatLexNormal(&tokens, &lexer, false, 2);
    RepeatLexCommand(&tokens, &lexer, 6);

    vector<Token> expectedTokens = {
        TextToken(U"  p$s"),
        DollarLBraceToken(),
        IdentifierToken(U"ccc"),
        RBraceToken(),
        TextToken(U" \"ddd "),
        IdentifierToken(U"e"),
        TextToken(U"  "),
        NewLineToken(),
        TextToken(U" "),
        RBraceToken(),
    };

    EXPECT_EQ(tokens, expectedTokens);
}

TEST(CommandLexer, LexCommands)
{
    auto [buffer, lexer] = Make(U"ls -al");
    auto tokens = Process(lexer);

    vector<Token> expectedTokens = {
        TextToken(U"ls -al")
    };

    EXPECT_EQ(tokens, expectedTokens);
}

TEST(CommandLexer, LexMultiLines)
{
    auto [buffer, lexer] = Make(UR"---(
hello world \n

    hello    

})---");

    vector<Token> tokens;
    RepeatLexCommand(&tokens, &lexer, 6);

    vector<Token> expected = {
        NewLineToken(),
        TextToken(U"hello world \\n"), NewLineToken(), // skip multi newlines
        TextToken(U"    hello    "), NewLineToken(),
        RBraceToken()
    };

    EXPECT_EQ(tokens, expected);
}

TEST(CommandLexer, LexCommandsWithLineSeparator)
{
    auto [buffer, lexer] = Make(U"ls -al\r\nbb");
    auto tokens = Process(lexer);

    vector<Token> expected = {
        TextToken(U"ls -al"),
        NewLineToken(),
        TextToken(U"bb"),
    };

    EXPECT_EQ(tokens, expected);
}