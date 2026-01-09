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
    auto o_lexResult = lexer->LexNormalMode(true);
    return Accept<TToken>(lexer, std::move(o_lexResult));
}

template<typename TToken>
bool Peek(Lexer& lexer)
{
    auto o_lexResult = lexer.LexNormalMode(true);
    return o_lexResult && std::holds_alternative<TToken>(o_lexResult->token);
}

template<typename TToken>
bool Peek(std::optional<LexResult> o_lexResult)
{
    return o_lexResult && std::holds_alternative<TToken>(o_lexResult->token);
}

enum class SParamModifier;
std::optional<SParamModifier> ParseParamModifier(Lexer* lexer);

}