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
std::optional<TLexResult<TToken>> Peek(Lexer& lexer)
{
    auto o_lexResult = lexer.LexNormalMode(true);
    if (!o_lexResult) return std::nullopt;

    auto* token = std::get_if<TToken>(&o_lexResult->token);
    if (!token) return std::nullopt;

    return TLexResult<TToken>(std::move(*token), std::move(o_lexResult->lexer));
}

template<typename TToken>
bool Peek(std::optional<LexResult> o_lexResult)
{
    return o_lexResult && std::holds_alternative<TToken>(o_lexResult->token);
}

template<typename TToken>
void Accept(Lexer* lexer, TLexResult<TToken>& result)
{
    *lexer = std::move(result.lexer);
}

enum class SParamModifier;
std::optional<SParamModifier> ParseParamModifier(Lexer* lexer);

}