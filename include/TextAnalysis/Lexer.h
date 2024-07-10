#pragma once
#include "TextAnalysisConfig.h"

#include <Syntax/Tokens.h>
#include <TextAnalysis/BufferPosition.h>

namespace Citron
{

struct LexResult;

// immutable for backtracking
class Lexer
{
    BufferPosition pos;

public:
    TEXTANALYSIS_API Lexer(BufferPosition pos);

public: // LexStringMode
    TEXTANALYSIS_API std::optional<LexResult> LexStringMode();
private:
    std::optional<LexResult> LexStringModeText();

public: // LexNormalMode
    TEXTANALYSIS_API std::optional<LexResult> LexNormalMode(bool bSkipNewLine);

private:
    std::optional<LexResult> LexNormalModeAfterSkipWhitespace();

public: // LexCommandMode
    TEXTANALYSIS_API std::optional<LexResult> LexCommandMode();

    TEXTANALYSIS_API bool IsReachedEnd();

public:
    std::optional<LexResult> LexIdentifier(bool bAllowRawMark);
    std::optional<LexResult> LexKeyword();
    std::optional<LexResult> LexBool();
    TEXTANALYSIS_API std::optional<LexResult> LexInt();
    TEXTANALYSIS_API std::optional<LexResult> LexWhitespace(bool bIncludeNewLine);
    TEXTANALYSIS_API std::optional<LexResult> LexNewLine();
};

struct LexResult
{
    Token token;
    Lexer lexer;
};


}