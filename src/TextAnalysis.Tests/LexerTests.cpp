#include "pch.h"
#include <TextAnalysis/Buffer.h>
#include <TextAnalysis/BufferPosition.h>
#include <TextAnalysis/Lexer.h>

#include "TestMisc.h"

//template<typename T, class... Types>
//inline bool operator==(const std::variant<Types...>& v, const T& t) {
//    const T* c = std::get_if<T>(&v);
//
//    return c && *c == t; // true if v contains a T that compares equal to t    
//}
//
//template<typename T, class... Types>
//inline bool operator==(const T& t, const std::variant<Types...>& v) {
//    return v == t;
//}


using namespace std;
using namespace Citron;

// template<optional<LexResult> (*Action)(Lexer* lexer)>
template<typename TFunc>
vector<Token> ProcessInner(TFunc Action, Lexer* lexer)
{
    vector<Token> result;

    while (true)
    {
        auto oLexResult = Action(lexer);

        if (!oLexResult) return result;
        if (holds_alternative<EndOfFileToken>(oLexResult->token)) break;

        *lexer = std::move(oLexResult->lexer);
        result.push_back(std::move(oLexResult->token));
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
        " enum struct class is as ref box local"
        " null public protected private static"
        " new namespace"
        " ++ -- <= >= => == != ->"
        " @ < > ; , = { } ( ) [ ] + - * / % ! . ? & : `");

    auto tokens = ProcessNormal(&lexer);

    vector<Token> expectedTokens = {
        IfToken(),
        ElseToken(),
        ForToken(),
        ContinueToken(),
        BreakToken(),
        TaskToken(),
        ParamsToken(),
        OutToken(),
        ReturnToken(),
        AsyncToken(),
        AwaitToken(),
        ForeachToken(),
        InToken(),
        YieldToken(),
        SeqToken(),
        EnumToken(),
        StructToken(),
        ClassToken(),
        IsToken(),
        AsToken(),
        RefToken(),
        BoxToken(),
        LocalToken(),
        NullToken(),

        PublicToken(),
        ProtectedToken(),
        PrivateToken(),
        StaticToken(),

        NewToken(),
        NamespaceToken(),

        PlusPlusToken(),
        MinusMinusToken(),
        LessThanEqualToken(),
        GreaterThanEqualToken(),
        EqualGreaterThanToken(),
        EqualEqualToken(),
        ExclEqualToken(),
        MinusGreaterThanToken(),

        AtToken(),
        LessThanToken(),
        GreaterThanToken(),
        SemiColonToken(),
        CommaToken(),
        EqualToken(),
        LBraceToken(),
        RBraceToken(),
        LParenToken(),
        RParenToken(),
        LBracketToken(),
        RBracketToken(),

        PlusToken(),
        MinusToken(),
        StarToken(),
        SlashToken(),
        PercentToken(),
        ExclToken(),
        DotToken(),
        QuestionToken(),
        AmpersandToken(),

        ColonToken(),
        BacktickToken(),
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
    auto oTokenResult = lexer.LexNormalMode(false);

    EXPECT_TRUE(oTokenResult);
    // EXPECT_EQ(oTokenResult->token, IdentifierToken(U"x"));
    EXPECT_EQ(IdentifierToken("x"), oTokenResult->token);
}

TEST(Lexer, LexNormalString)
{
    auto [buffer, lexer] = Prepare(U"  \"aaa bbb \"  ");
    
    auto oResult0 = lexer.LexNormalMode(false);
    ASSERT_TRUE(oResult0);

    auto oResult1 = oResult0->lexer.LexStringMode();
    ASSERT_TRUE(oResult1);

    auto oResult2 = oResult1->lexer.LexStringMode();
    ASSERT_TRUE(oResult2);

    EXPECT_EQ(oResult0->token, DoubleQuoteToken());
    EXPECT_EQ(oResult1->token, TextToken("aaa bbb "));
    EXPECT_EQ(oResult2->token, DoubleQuoteToken());
}

// stringMode
TEST(Lexer, LexDoubleQuoteString)
{
    auto [buffer, lexer] = Prepare(U"\"\"");
    auto oTokenResult = lexer.LexStringMode();

    auto expectedToken = TextToken("\"");

    EXPECT_EQ(oTokenResult->token, expectedToken);
}

TEST(Lexer, LexDollarString)
{
    auto [buffer, lexer] = Prepare(U"$$");

    auto oResult = lexer.LexStringMode();
    ASSERT_TRUE(oResult);

    auto expectedToken = TextToken("$");

    EXPECT_EQ(oResult->token, expectedToken);
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

    auto oResult = lexer.LexStringMode();
    tokens.push_back(std::move(oResult->token));

    oResult = oResult->lexer.LexStringMode();
    tokens.push_back(std::move(oResult->token));

    oResult = oResult->lexer.LexNormalMode(false);
    tokens.push_back(std::move(oResult->token));

    oResult = oResult->lexer.LexNormalMode(false);
    tokens.push_back(std::move(oResult->token));

    oResult = oResult->lexer.LexStringMode();
    tokens.push_back(std::move(oResult->token));

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

    auto oResult = lexer.LexNormalMode(false);
    tokens.push_back(oResult->token); // "

    oResult = oResult->lexer.LexStringMode();
    tokens.push_back(oResult->token); // aaa bbb

    oResult = oResult->lexer.LexStringMode();
    tokens.push_back(oResult->token); // ${

    oResult = oResult->lexer.LexNormalMode(false);
    tokens.push_back(oResult->token); // "

    oResult = oResult->lexer.LexStringMode();
    tokens.push_back(oResult->token); // xxx 

    oResult = oResult->lexer.LexStringMode();
    tokens.push_back(oResult->token); // ${

    oResult = oResult->lexer.LexNormalMode(false);
    tokens.push_back(oResult->token); // ddd

    oResult = oResult->lexer.LexNormalMode(false);
    tokens.push_back(oResult->token); // }

    oResult = oResult->lexer.LexStringMode();
    tokens.push_back(oResult->token); // "

    oResult = oResult->lexer.LexNormalMode(false);
    tokens.push_back(oResult->token); // }

    oResult = oResult->lexer.LexStringMode();
    tokens.push_back(oResult->token); // ddd 

    oResult = oResult->lexer.LexStringMode();
    tokens.push_back(oResult->token); // "

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

    auto oResult = lexer.LexNormalMode(false);
    ASSERT_TRUE(oResult);

    auto expectedToken = IntToken(1234);

    EXPECT_EQ(oResult->token, expectedToken);
}

TEST(Lexer, LexComment)
{
    auto [buffer, lexer] = Prepare(U"  // e s \r\n// \r// \n1234"); // 나머지는 지원 안함    

    vector<Token> tokens;

    auto oResult = lexer.LexWhitespace(false);
    tokens.push_back(oResult->token);

    oResult = oResult->lexer.LexNewLine();
    tokens.push_back(oResult->token);

    oResult = oResult->lexer.LexWhitespace(false);
    tokens.push_back(oResult->token);

    oResult = oResult->lexer.LexNewLine();
    tokens.push_back(oResult->token);

    oResult = oResult->lexer.LexWhitespace(false);
    tokens.push_back(oResult->token);

    oResult = oResult->lexer.LexNewLine();
    tokens.push_back(oResult->token);

    oResult = oResult->lexer.LexInt();
    tokens.push_back(oResult->token);

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

    auto oResult = lexer.LexInt();
    tokens.push_back(oResult->token);

    oResult = oResult->lexer.LexWhitespace(false);
    tokens.push_back(oResult->token);

    oResult = oResult->lexer.LexInt();
    tokens.push_back(oResult->token);

    vector<Token> expectedTokens{
        IntToken(1234),
        WhitespaceToken(),
        IntToken(55)
    };

    EXPECT_EQ(tokens, expectedTokens);
}

