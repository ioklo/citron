#pragma once
#include "SyntaxConfig.h"

#include <string>
#include <variant>

namespace Citron::Tokens {

// i love macro
#define TOKEN_DEF(name, text) struct name { const wchar_t* DebugText = text; };

TOKEN_DEF(EqualEqual, L"==")
TOKEN_DEF(ExclEqual, L"!=")
TOKEN_DEF(PlusPlus, L"++") // ++
TOKEN_DEF(MinusMinus, L"--") // --
TOKEN_DEF(LessThanEqual, L"<=") // <=
TOKEN_DEF(GreaterThanEqual, L">=") // >=

TOKEN_DEF(EqualGreaterThan, L"=>") // =>
TOKEN_DEF(MinusGreaterThan, L"->") // ->
TOKEN_DEF(LessThan, L"<") // <
TOKEN_DEF(GreaterThan, L">") // >
TOKEN_DEF(Equal, L"=") // =
TOKEN_DEF(Comma, L",") // ,
TOKEN_DEF(SemiColon, L";") // ;   
TOKEN_DEF(LBrace, L"{") // {
TOKEN_DEF(RBrace, L"}") // }
TOKEN_DEF(LParen, L"(") // (
TOKEN_DEF(RParen, L")") // )
TOKEN_DEF(LBracket, L"[") // [
TOKEN_DEF(RBracket, L"]") // ]
TOKEN_DEF(Plus, L"+") // +
TOKEN_DEF(Minus, L"-") // -
TOKEN_DEF(Star, L"*") // *   
TOKEN_DEF(Slash, L"/") // /    
TOKEN_DEF(Percent, L"%") // %    
TOKEN_DEF(Excl, L"!") // !    
TOKEN_DEF(Dot, L".") // .
TOKEN_DEF(Question, L"?") // ?
TOKEN_DEF(Ampersand, L"&") // &
TOKEN_DEF(If, L"if")
TOKEN_DEF(Else, L"else")
TOKEN_DEF(For, L"for")
TOKEN_DEF(Continue, L"continue")
TOKEN_DEF(Break, L"break")
TOKEN_DEF(Task, L"task")
TOKEN_DEF(Params, L"params")
TOKEN_DEF(Out, L"out")
TOKEN_DEF(Return, L"return")
TOKEN_DEF(Async, L"async")
TOKEN_DEF(Await, L"await")
TOKEN_DEF(Foreach, L"foreach")
TOKEN_DEF(In, L"in")
TOKEN_DEF(Yield, L"yield")
TOKEN_DEF(Seq, L"seq")
TOKEN_DEF(Enum, L"enum")
TOKEN_DEF(Struct, L"struct")
TOKEN_DEF(Class, L"class")
TOKEN_DEF(Is, L"is")
TOKEN_DEF(As, L"as")
TOKEN_DEF(Ref, L"ref")
TOKEN_DEF(Box, L"box")
TOKEN_DEF(Local, L"local")
TOKEN_DEF(Null, L"null")

TOKEN_DEF(Public, L"public")
TOKEN_DEF(Protected, L"protected")
TOKEN_DEF(Private, L"private")
TOKEN_DEF(Static, L"static")
TOKEN_DEF(New, L"new")
TOKEN_DEF(Namespace, L"namespace")

TOKEN_DEF(Colon, L":")
TOKEN_DEF(Backtick, L"`")
TOKEN_DEF(Whitespace, L"<whitespace>") // \s
TOKEN_DEF(NewLine, L"<newline>")     // \r \n \r\n

// SingleToken DoubleQuote("\"") // "
TOKEN_DEF(DollarLBrace, L"${")
TOKEN_DEF(EndOfFile, L"<eof>")
TOKEN_DEF(At, L"@")

// digit
struct IntToken
{
    int value;
    IntToken(int value) : value(value) { }
};

struct BoolToken
{
    bool value;
    BoolToken(bool value) : value(value) { }
};

struct TextToken
{
    std::string text;
    TextToken(const std::string& text) : text(text) { }
};

struct IdentifierToken
{
    std::string text;
    IdentifierToken(const std::string& text) : text(text) { }
};

using Token = std::variant <
    EqualEqual,
    ExclEqual,

    PlusPlus,
    MinusMinus,
    LessThanEqual,
    GreaterThanEqual,

    EqualGreaterThan,
    MinusGreaterThan,
    LessThan,
    GreaterThan,
    Equal,
    Comma,
    SemiColon,
    LBrace,
    RBrace,
    LParen,
    RParen,
    LBracket,
    RBracket,
    Plus,
    Minus,
    Star,
    Slash,
    Percent,
    Excl,
    Dot,
    Question,
    Ampersand,
    If,
    Else,
    For,
    Continue,
    Break,
    Task,
    Params,
    Out,
    Return,
    Async,
    Await,
    Foreach,
    In,
    Yield,
    Seq,
    Enum,
    Struct,
    Class,
    Is,
    As,
    Ref,
    Box,
    Local,
    Null,

    Public,
    Protected,
    Private,
    Static,
    New,
    Namespace,

    Colon,
    Backtick,
    Whitespace,
    NewLine,
    DollarLBrace,
    EndOfFile,
    At,

    IntToken,
    BoolToken,
    TextToken,
    IdentifierToken
>;

}