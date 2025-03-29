export module Citron.ParserMisc;

import <optional>;
import <variant>;
import Citron.Tokens;
import Citron.Lexer;

namespace Citron {

export template<typename TToken>
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

export template<typename TToken>
std::optional<TToken> Accept(Lexer* lexer)
{
    auto oLexResult = lexer->LexNormalMode(true);
    return Accept<TToken>(lexer, std::move(oLexResult));
}

export template<typename TToken>
bool Peek(Lexer& lexer)
{
    auto oLexResult = lexer.LexNormalMode(true);
    return oLexResult && std::holds_alternative<TToken>(oLexResult->token);
}

export template<typename TToken>
bool Peek(std::optional<LexResult> oLexResult)
{
    return oLexResult && std::holds_alternative<TToken>(oLexResult->token);
}

export struct OutAndParams
{
    bool bOut;
    bool bParams;
};

export std::optional<OutAndParams> AcceptParseOutAndParams(Lexer* lexer);

}