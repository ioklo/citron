#pragma once
#include "SyntaxConfig.h"

#include <string>
#include <variant>

namespace Citron {

#define TOKEN_DEF(name, text) struct name { \
    inline static const wchar_t* DebugText = text; \
    bool operator==(const name&) const { return true; } \
};

TOKEN_DEF(EqualEqualToken, L"==")
TOKEN_DEF(ExclEqualToken, L"!=")
TOKEN_DEF(PlusPlusToken, L"++") // ++
TOKEN_DEF(MinusMinusToken, L"--") // --
TOKEN_DEF(LessThanEqualToken, L"<=") // <=
TOKEN_DEF(GreaterThanEqualToken, L">=") // >=

TOKEN_DEF(EqualGreaterThanToken, L"=>") // =>
TOKEN_DEF(MinusGreaterThanToken, L"->") // ->
TOKEN_DEF(LessThanToken, L"<") // <
TOKEN_DEF(GreaterThanToken, L">") // >
TOKEN_DEF(EqualToken, L"=") // =
TOKEN_DEF(CommaToken, L",") // ,
TOKEN_DEF(SemiColonToken, L";") // ;   
TOKEN_DEF(LBraceToken, L"{") // {
TOKEN_DEF(RBraceToken, L"}") // }
TOKEN_DEF(LParenToken, L"(") // (
TOKEN_DEF(RParenToken, L")") // )
TOKEN_DEF(LBracketToken, L"[") // [
TOKEN_DEF(RBracketToken, L"]") // ]
TOKEN_DEF(PlusToken, L"+") // +
TOKEN_DEF(MinusToken, L"-") // -
TOKEN_DEF(StarToken, L"*") // *   
TOKEN_DEF(SlashToken, L"/") // /    
TOKEN_DEF(PercentToken, L"%") // %    
TOKEN_DEF(ExclToken, L"!") // !    
TOKEN_DEF(DotToken, L".") // .
TOKEN_DEF(QuestionToken, L"?") // ?
TOKEN_DEF(AmpersandToken, L"&") // &
TOKEN_DEF(IfToken, L"if")
TOKEN_DEF(ElseToken, L"else")
TOKEN_DEF(ForToken, L"for")
TOKEN_DEF(ContinueToken, L"continue")
TOKEN_DEF(BreakToken, L"break")
TOKEN_DEF(TaskToken, L"task")
TOKEN_DEF(ParamsToken, L"params")
TOKEN_DEF(OutToken, L"out")
TOKEN_DEF(ReturnToken, L"return")
TOKEN_DEF(AsyncToken, L"async")
TOKEN_DEF(AwaitToken, L"await")
TOKEN_DEF(ForeachToken, L"foreach")
TOKEN_DEF(InToken, L"in")
TOKEN_DEF(YieldToken, L"yield")
TOKEN_DEF(SeqToken, L"seq")
TOKEN_DEF(EnumToken, L"enum")
TOKEN_DEF(StructToken, L"struct")
TOKEN_DEF(ClassToken, L"class")
TOKEN_DEF(IsToken, L"is")
TOKEN_DEF(AsToken, L"as")
TOKEN_DEF(RefToken, L"ref")
TOKEN_DEF(BoxToken, L"box")
TOKEN_DEF(LocalToken, L"local")
TOKEN_DEF(NullToken, L"null")

TOKEN_DEF(PublicToken, L"public")
TOKEN_DEF(ProtectedToken, L"protected")
TOKEN_DEF(PrivateToken, L"private")
TOKEN_DEF(StaticToken, L"static")
TOKEN_DEF(NewToken, L"new")
TOKEN_DEF(NamespaceToken, L"namespace")

TOKEN_DEF(ColonToken, L":")
TOKEN_DEF(BacktickToken, L"`")
TOKEN_DEF(WhitespaceToken, L"<whitespace>") // \s
TOKEN_DEF(NewLineToken, L"<newline>")     // \r \n \r\n

TOKEN_DEF(DoubleQuoteToken, L"\"") // "
TOKEN_DEF(DollarLBraceToken, L"${")
TOKEN_DEF(EndOfFileToken, L"<eof>")
TOKEN_DEF(AtToken, L"@")

// digit
struct IntToken
{
    int value;
    IntToken(int value) : value(value) { }
    bool operator==(const IntToken& other) const { return value == other.value; }
};

struct BoolToken
{
    bool value;

    BoolToken(bool value) : value(value) { }
    bool operator==(const BoolToken& other) const { return value == other.value; }
};

struct TextToken
{
    std::u32string text;
    TextToken(std::u32string text) : text(std::move(text)) { }
    bool operator==(const TextToken& other) const { return text == other.text; }
};

struct IdentifierToken
{
    std::u32string text;
    IdentifierToken(std::u32string text) : text(std::move(text)) { }
    bool operator==(const IdentifierToken& other) const { return text == other.text; }
};

using Token = std::variant <
    EqualEqualToken,
    ExclEqualToken,

    PlusPlusToken,
    MinusMinusToken,
    LessThanEqualToken,
    GreaterThanEqualToken,

    EqualGreaterThanToken,
    MinusGreaterThanToken,
    LessThanToken,
    GreaterThanToken,
    EqualToken,
    CommaToken,
    SemiColonToken,
    LBraceToken,
    RBraceToken,
    LParenToken,
    RParenToken,
    LBracketToken,
    RBracketToken,
    PlusToken,
    MinusToken,
    StarToken,
    SlashToken,
    PercentToken,
    ExclToken,
    DotToken,
    QuestionToken,
    AmpersandToken,
    IfToken,
    ElseToken,
    ForToken,
    ContinueToken,
    BreakToken,
    TaskToken,
    ParamsToken,
    OutToken,
    ReturnToken,
    AsyncToken,
    AwaitToken,
    ForeachToken,
    InToken,
    YieldToken,
    SeqToken,
    EnumToken,
    StructToken,
    ClassToken,
    IsToken,
    AsToken,
    RefToken,
    BoxToken,
    LocalToken,
    NullToken,

    PublicToken,
    ProtectedToken,
    PrivateToken,
    StaticToken,
    NewToken,
    NamespaceToken,

    ColonToken,
    BacktickToken,
    WhitespaceToken,
    NewLineToken,

    DoubleQuoteToken,
    DollarLBraceToken,
    EndOfFileToken,
    AtToken,

    IntToken,
    BoolToken,
    TextToken,
    IdentifierToken
>;

template<typename TToken, typename = std::enable_if_t<!std::is_same_v<TToken, Token> && std::is_assignable_v<Token, TToken>>>
bool operator==(const Token& token1, const TToken& token2)
{
    const TToken* c = std::get_if<TToken>(&token1);
    return c && *c == token2; // true if v contains a T that compares equal to t    
}

template<typename TToken, typename = std::enable_if_t<!std::is_same_v<TToken, Token> && std::is_assignable_v<Token, TToken>>>
bool operator==(const TToken& token1, const Token& token2)
{
    const TToken* c = std::get_if<TToken>(&token2);
    return c && *c == token1; // true if v contains a T that compares equal to t    
}

}

