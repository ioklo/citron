#include <gtest/gtest.h>

#include <variant>
#include <vector>
#include <optional>

#include "Syntax/Tokens.h"
#include "TextAnalysis/Buffer.h"
#include "TextAnalysis/Lexer.h"

#include "TestMisc.h"



using namespace std;
using namespace Citron;

// template<optional<LexResult> (*Action)(Lexer* lexer)>
template<typename TFunc>
vector<Token> ProcessInner(TFunc Action, Lexer* lexer)
{
    vector<Token> result;

    while (true)
    {
        auto o_lexResult = Action(lexer);

        if (!o_lexResult) return result;
        if (holds_alternative<EndOfFileToken>(o_lexResult->token)) break;

        *lexer = move(o_lexResult->lexer);
        result.push_back(move(o_lexResult->token));
    }

    return result;
}

optional<LexResult> LexNormalModeWrapper(Lexer* lexer)
{
    return lexer->LexNormalMode(false);
}

vector<Token> ProcessNormal(Lexer* lexer)
{
    return ProcessInner([](Lexer* lexer) {return lexer->LexNormalMode(false); }, lexer);
}

vector<Token> ProcessString(Lexer* lexer)
{
    return ProcessInner([](Lexer* lexer) {return lexer->LexStringMode(); }, lexer);
}

TEST(Lexer, LexSymbols)
{
    auto [buffer, lexer] = Prepare(U"if else for continue break task params out return async await foreach in yield seq"
        " enum struct class is as ref"
        " null public protected private static"
        " new namespace"
        " ++ -- <= >= => == != ->"
        " @ < > ; , = { } ( ) [ ] + - * / % ! . ? & ~ : `");

    auto tokens = ProcessNormal(&lexer);

    vector<Token> expectedTokens = {
        IfToken{},
        ElseToken{},
        ForToken{},
        ContinueToken{},
        BreakToken{},
        TaskToken{},
        ParamsToken{},
        OutToken{},
        ReturnToken{},
        AsyncToken{},
        AwaitToken{},
        ForeachToken{},
        InToken{},
        YieldToken{},
        SeqToken{},
        EnumToken{},
        StructToken{},
        ClassToken{},
        IsToken{},
        AsToken{},
        RefToken{},
        NullToken{},

        PublicToken{},
        ProtectedToken{},
        PrivateToken{},
        StaticToken{},

        NewToken{},
        NamespaceToken{},

        PlusPlusToken{},
        MinusMinusToken{},
        LessThanEqualToken{},
        GreaterThanEqualToken{},
        EqualGreaterThanToken{},
        EqualEqualToken{},
        ExclEqualToken{},
        MinusGreaterThanToken{},

        AtToken{},
        LessThanToken{},
        GreaterThanToken{},
        SemiColonToken{},
        CommaToken{},
        EqualToken{},
        LBraceToken{},
        RBraceToken{},
        LParenToken{},
        RParenToken{},
        LBracketToken{},
        RBracketToken{},

        PlusToken{},
        MinusToken{},
        StarToken{},
        SlashToken{},
        PercentToken{},
        ExclToken{},
        DotToken{},
        QuestionToken{},
        AmpersandToken{},
        TildeToken{},

        ColonToken{},
        BacktickToken{},
    };

    EXPECT_EQ(tokens, expectedTokens);
}

TEST(Lexer, LexKeywords)
{
    auto [buffer, lexer] = Prepare(U"true false");

    auto tokens = ProcessNormal(&lexer);
    auto expectedTokens = vector<Token> {
        BoolToken(true),
        BoolToken(false),
    };

    EXPECT_EQ(tokens, expectedTokens);
}

TEST(Lexer, LexSimpleIdentifier)
{
    auto [buffer, lexer] = Prepare(U"x");    
    auto o_tokenResult = lexer.LexNormalMode(false);

    EXPECT_TRUE(o_tokenResult);
    // EXPECT_EQ(o_tokenResult->token, IdentifierToken("x"));

    EXPECT_EQ(IdentifierToken("x"), o_tokenResult->token);
}

TEST(Lexer, LexNormalString)
{
    auto [buffer, lexer] = Prepare(U"  \"aaa bbb \"  ");
    
    auto o_result0 = lexer.LexNormalMode(false);
    ASSERT_TRUE(o_result0);

    auto o_result1 = o_result0->lexer.LexStringMode();
    ASSERT_TRUE(o_result1);

    auto o_result2 = o_result1->lexer.LexStringMode();
    ASSERT_TRUE(o_result2);

    EXPECT_EQ(o_result0->token, DoubleQuoteToken());
    EXPECT_EQ(o_result1->token, TextToken("aaa bbb "));
    EXPECT_EQ(o_result2->token, DoubleQuoteToken());
}

// stringMode
TEST(Lexer, LexDoubleQuoteString)
{
    auto [buffer, lexer] = Prepare(U"\"\"");
    auto o_tokenResult = lexer.LexStringMode();

    auto expectedToken = TextToken("\"");

    EXPECT_EQ(o_tokenResult->token, expectedToken);
}

TEST(Lexer, LexDollarString)
{
    auto [buffer, lexer] = Prepare(U"$$");

    auto o_result = lexer.LexStringMode();
    ASSERT_TRUE(o_result);

    auto expectedToken = TextToken("$");

    EXPECT_EQ(o_result->token, expectedToken);
}

TEST(Lexer, LexSimpleEscapedString2)
{
    auto [buffer, lexer] = Prepare(U"$ccc");

    auto oResult = lexer.LexStringMode();
    auto expectedToken = IdentifierToken("ccc");

    EXPECT_EQ(oResult->token, expectedToken);
}

TEST(Lexer, LexSimpleEscapedString)
{
    auto [buffer, lexer] = Prepare(U"aaa bbb $ccc ddd");

    auto tokens = ProcessString(&lexer);

    auto expectedTokens = vector<Token>{
        TextToken("aaa bbb "),
        IdentifierToken("ccc"),
        TextToken(" ddd"),
    };

    EXPECT_EQ(tokens, expectedTokens);
}

TEST(Lexer, LexEscapedString)
{
    auto [buffer, lexer] = Prepare(U"aaa bbb ${ccc} ddd"); // TODO: "aaa bbb ${ ccc \r\n } ddd" 는 에러

    vector<Token> tokens;

    auto o_result = lexer.LexStringMode();
    tokens.push_back(move(o_result->token));

    o_result = o_result->lexer.LexStringMode();
    tokens.push_back(move(o_result->token));

    o_result = o_result->lexer.LexNormalMode(false);
    tokens.push_back(move(o_result->token));

    o_result = o_result->lexer.LexNormalMode(false);
    tokens.push_back(move(o_result->token));

    o_result = o_result->lexer.LexStringMode();
    tokens.push_back(move(o_result->token));

    vector<Token> expectedTokens {
        TextToken("aaa bbb "),
        DollarLBraceToken(),
        IdentifierToken("ccc"),
        RBraceToken(),
        TextToken(" ddd"),
    };

    EXPECT_EQ(tokens, expectedTokens);
}

TEST(Lexer, LexComplexString)
{
    auto [buffer, lexer] = Prepare(U"\"aaa bbb ${\"xxx ${ddd}\"} ddd\"");

    vector<Token> tokens;

    auto o_result = lexer.LexNormalMode(false);
    tokens.push_back(o_result->token); // "

    o_result = o_result->lexer.LexStringMode();
    tokens.push_back(o_result->token); // aaa bbb

    o_result = o_result->lexer.LexStringMode();
    tokens.push_back(o_result->token); // ${

    o_result = o_result->lexer.LexNormalMode(false);
    tokens.push_back(o_result->token); // "

    o_result = o_result->lexer.LexStringMode();
    tokens.push_back(o_result->token); // xxx 

    o_result = o_result->lexer.LexStringMode();
    tokens.push_back(o_result->token); // ${

    o_result = o_result->lexer.LexNormalMode(false);
    tokens.push_back(o_result->token); // ddd

    o_result = o_result->lexer.LexNormalMode(false);
    tokens.push_back(o_result->token); // }

    o_result = o_result->lexer.LexStringMode();
    tokens.push_back(o_result->token); // "

    o_result = o_result->lexer.LexNormalMode(false);
    tokens.push_back(o_result->token); // }

    o_result = o_result->lexer.LexStringMode();
    tokens.push_back(o_result->token); // ddd 

    o_result = o_result->lexer.LexStringMode();
    tokens.push_back(o_result->token); // "

    vector<Token> expectedTokens {
        DoubleQuoteToken(),
        TextToken("aaa bbb "),
        DollarLBraceToken(),

        DoubleQuoteToken(),

        TextToken("xxx "),
        DollarLBraceToken(),
        IdentifierToken("ddd"),
        RBraceToken(),
        DoubleQuoteToken(),
        RBraceToken(),
        TextToken(" ddd"),
        DoubleQuoteToken(),
    };

    EXPECT_EQ(tokens, expectedTokens);
}

TEST(Lexer, LexInt)
{
    auto [buffer, lexer] = Prepare(U"1234"); // 나머지는 지원 안함

    auto o_result = lexer.LexNormalMode(false);
    ASSERT_TRUE(o_result);

    auto expectedToken = IntToken(1234);

    EXPECT_EQ(o_result->token, expectedToken);
}

TEST(Lexer, LexComment)
{
    auto [buffer, lexer] = Prepare(U"  // e s \r\n// \r// \n1234"); // 나머지는 지원 안함    

    vector<Token> tokens;

    auto o_result = lexer.LexWhitespace(false);
    tokens.push_back(o_result->token);

    o_result = o_result->lexer.LexNewLine();
    tokens.push_back(o_result->token);

    o_result = o_result->lexer.LexWhitespace(false);
    tokens.push_back(o_result->token);

    o_result = o_result->lexer.LexNewLine();
    tokens.push_back(o_result->token);

    o_result = o_result->lexer.LexWhitespace(false);
    tokens.push_back(o_result->token);

    o_result = o_result->lexer.LexNewLine();
    tokens.push_back(o_result->token);

    o_result = o_result->lexer.LexInt();
    tokens.push_back(o_result->token);

    vector<Token> expectedTokens{
        WhitespaceToken(),
        NewLineToken(),
        WhitespaceToken(),
        NewLineToken(),
        WhitespaceToken(),
        NewLineToken(),
        IntToken(1234)
    };

    EXPECT_EQ(tokens, expectedTokens);
}

TEST(Lexer, LexNextLine)
{
    auto [buffer, lexer] = Prepare(U"1234 \\ // comment \r\n 55"); // 나머지는 지원 안함

    vector<Token> tokens;

    auto o_result = lexer.LexInt();
    tokens.push_back(o_result->token);

    o_result = o_result->lexer.LexWhitespace(false);
    tokens.push_back(o_result->token);

    o_result = o_result->lexer.LexInt();
    tokens.push_back(o_result->token);

    vector<Token> expectedTokens{
        IntToken(1234),
        WhitespaceToken(),
        IntToken(55)
    };

    EXPECT_EQ(tokens, expectedTokens);
}

