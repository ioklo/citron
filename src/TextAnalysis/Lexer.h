#pragma once

#include "Syntax/Tokens.h"
#include "BufferPosition.h"

namespace Citron
{

struct LexResult;

// immutable for backtracking
class Lexer
{
    BufferPosition pos;

public:
    Lexer(BufferPosition pos);

public: // LexStringMode
    std::optional<LexResult> LexStringMode();
private:
    std::optional<LexResult> LexStringModeText();

public: // LexNormalMode
    std::optional<LexResult> LexNormalMode(bool bSkipNewLine);

private:
    std::optional<LexResult> LexNormalModeAfterSkipWhitespace();

public: // LexCommandMode
    std::optional<LexResult> LexCommandMode();

    bool IsReachedEnd();

private:
    std::optional<LexResult> LexIdentifier(bool bAllowRawMark);
    std::optional<LexResult> LexKeyword();
    std::optional<LexResult> LexBool();
    std::optional<LexResult> LexInt();
    std::optional<LexResult> LexWhitespace(bool bIncludeNewLine);
    std::optional<LexResult> LexNewLine();

    std::optional<BufferPosition> Consume(const char32_t* sequence);
};

struct LexResult
{
    Token token;
    Lexer lexer;
};


}