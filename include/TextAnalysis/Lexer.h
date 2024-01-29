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
    TEXT_ANALYSIS_API Lexer(BufferPosition pos);

public: // LexStringMode
    TEXT_ANALYSIS_API std::optional<LexResult> LexStringMode();
private:
    std::optional<LexResult> LexStringModeText();

public: // LexNormalMode
    TEXT_ANALYSIS_API std::optional<LexResult> LexNormalMode(bool bSkipNewLine);

private:
    std::optional<LexResult> LexNormalModeAfterSkipWhitespace();

public: // LexCommandMode
    TEXT_ANALYSIS_API std::optional<LexResult> LexCommandMode();

    TEXT_ANALYSIS_API bool IsReachedEnd();

public:
    std::optional<LexResult> LexIdentifier(bool bAllowRawMark);
    std::optional<LexResult> LexKeyword();
    std::optional<LexResult> LexBool();
    TEXT_ANALYSIS_API std::optional<LexResult> LexInt();
    TEXT_ANALYSIS_API std::optional<LexResult> LexWhitespace(bool bIncludeNewLine);
    TEXT_ANALYSIS_API std::optional<LexResult> LexNewLine();
};

struct LexResult
{
    Token token;
    Lexer lexer;
};


}