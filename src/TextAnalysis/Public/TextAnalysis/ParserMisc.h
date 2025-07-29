#pragma once

#include <optional>
#include <variant>

#include "Syntax/Tokens.h"
#include "Lexer.h"

namespace Citron {

template<typename TToken>
std::optional<TToken> Accept(Lexer* lexer, std::optional<LexResult> lexResult)
{
    if (lexResult)
    {
        if (auto* token = std::get_if<TToken>(&lexResult->token))
        {
            *lexer = std::move(lexResult->lexer);
            return std::move(*token);
        }
    }

    return std::nullopt;
}

template<typename TToken>
std::optional<TToken> Accept(Lexer* lexer)
{
    auto oLexResult = lexer->LexNormalMode(true);
    return Accept<TToken>(lexer, std::move(oLexResult));
}

template<typename TToken>
bool Peek(Lexer& lexer)
{
    auto oLexResult = lexer.LexNormalMode(true);
    return oLexResult && std::holds_alternative<TToken>(oLexResult->token);
}

template<typename TToken>
bool Peek(std::optional<LexResult> oLexResult)
{
    return oLexResult && std::holds_alternative<TToken>(oLexResult->token);
}

struct OutAndParams
{
    bool bOut;
    bool bParams;
};

std::optional<OutAndParams> AcceptParseOutAndParams(Lexer* lexer);

}