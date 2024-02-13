#include "pch.h"
#include <TextAnalysis/TypeExpParser.h>

#include <optional>
#include <vector>

#include <Syntax/Syntax.h>
#include <Syntax/Tokens.h>

#include <TextAnalysis/Lexer.h>
#include "ParserMisc.h"

using namespace std;

namespace Citron {

optional<vector<TypeExpSyntax>> ParseTypeArgs(Lexer* lexer)
{
    vector<TypeExpSyntax> typeArgs;

    Lexer curLexer = *lexer;

    if (!Accept<LessThanToken>(&curLexer))
        return nullopt;

    while (!Accept<GreaterThanToken>(&curLexer))
    {
        if (!typeArgs.empty())
            if (!Accept<CommaToken>(&curLexer))
                return nullopt;

        auto oTypeArg = ParseTypeExp(&curLexer);
        if (!oTypeArg)
            return nullopt;

        typeArgs.push_back(std::move(*oTypeArg));
    }

    *lexer = std::move(curLexer);
    return typeArgs;
}

optional<IdTypeExpSyntax> ParseIdTypeExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oIdToken = Accept<IdentifierToken>(&curLexer);
    if (!oIdToken)
        return nullopt;

    if (auto oTypeArgs = ParseTypeArgs(&curLexer))
    {
        *lexer = std::move(curLexer);
        return IdTypeExpSyntax(oIdToken->text, std::move(*oTypeArgs));
    }
    else
    {
        *lexer = std::move(curLexer);
        return IdTypeExpSyntax(oIdToken->text, {});
    }
}

// T?
optional<NullableTypeExpSyntax> ParseNullableTypeExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    optional<TypeExpSyntax> oTypeExp = ParseBoxPtrTypeExp(&curLexer);
    if (!oTypeExp) oTypeExp = ParseLocalPtrTypeExp(&curLexer);
    if (!oTypeExp) oTypeExp = ParseParenTypeExp(&curLexer);
    if (!oTypeExp) oTypeExp = ParseIdChainTypeExp(&curLexer);
    if (!oTypeExp) return nullopt;

    if (!Accept<QuestionToken>(&curLexer))
        return nullopt;

    *lexer = std::move(curLexer);

    return NullableTypeExpSyntax(std::move(*oTypeExp));
}

// box T*
optional<BoxPtrTypeExpSyntax> ParseBoxPtrTypeExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<BoxToken>(&curLexer))
        return nullopt;

    optional<TypeExpSyntax> oTypeExp = ParseParenTypeExp(&curLexer);
    if (!oTypeExp) oTypeExp = ParseIdChainTypeExp(&curLexer);
    if (!oTypeExp) return nullopt;

    if (!Accept<StarToken>(&curLexer))
        return nullopt;

    *lexer = std::move(curLexer);
    return BoxPtrTypeExpSyntax(std::move(*oTypeExp));
}

// T*
optional<TypeExpSyntax> ParseLocalPtrTypeExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // avoid left recursion

    optional<TypeExpSyntax> oInnerTypeExp = ParseParenTypeExp(&curLexer);
    if (!oInnerTypeExp) oInnerTypeExp = ParseIdChainTypeExp(&curLexer);
    if (!oInnerTypeExp) return nullopt;
    
    // 적어도 한개는 있어야 한다
    if (!Accept<StarToken>(&curLexer))
        return nullopt;
    
    TypeExpSyntax curTypeExp = LocalPtrTypeExpSyntax(std::move(*oInnerTypeExp));

    while (Accept<StarToken>(&curLexer))
    {
        // NOTICE: LocalPtrTypeExpSyntax(std::move(curTypeExp)); curTypeExp가 LocalPtrTypeExpSyntax라면 감싸는게 아니라 이동생성자가 호출된다
        curTypeExp = LocalPtrTypeExpSyntax(TypeExpSyntax(std::move(curTypeExp)));
    }

    *lexer = std::move(curLexer);
    return curTypeExp;
}

// (T)
optional<TypeExpSyntax> ParseParenTypeExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<LParenToken>(&curLexer))
        return nullopt;

    optional<TypeExpSyntax> oInnerTypeExp = ParseNullableTypeExp(&curLexer);
    if (!oInnerTypeExp) oInnerTypeExp = ParseBoxPtrTypeExp(&curLexer);
    if (!oInnerTypeExp) oInnerTypeExp = ParseLocalPtrTypeExp(&curLexer);
    if (!oInnerTypeExp) oInnerTypeExp = ParseLocalTypeExp(&curLexer);
    if (!oInnerTypeExp) return nullopt;
    
    if (!Accept<RParenToken>(&curLexer))
        return nullopt;

    *lexer = std::move(curLexer);
    return oInnerTypeExp;
}

// ID...
optional<TypeExpSyntax> ParseIdChainTypeExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oIdTypeExp = ParseIdTypeExp(&curLexer);
    if (!oIdTypeExp)
        return nullopt;

    TypeExpSyntax curTypeExp = std::move(*oIdTypeExp);

    // .
    while (Accept<DotToken>(&curLexer))
    {
        // ID
        auto oIdToken = Accept<IdentifierToken>(&curLexer);
        if (!oIdToken)
            return nullopt;

        auto oTypeArgs = ParseTypeArgs(&curLexer);
        if (oTypeArgs)
            curTypeExp = MemberTypeExpSyntax(std::move(curTypeExp), std::move(oIdToken->text), std::move(*oTypeArgs));
        else 
            curTypeExp = MemberTypeExpSyntax(std::move(curTypeExp), std::move(oIdToken->text), {});
    }

    *lexer = std::move(curLexer);
    return curTypeExp;
}

// func<>
// std::optional<FuncTypeExpSyntax> ParseFuncTypeExp(Lexer* lexer);

// tuple
// std::optional<TupleTypeExpSyntax> ParseTupleTypeExp(Lexer* lexer);

// local I i;
optional<LocalTypeExpSyntax> ParseLocalTypeExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<LocalToken>(&curLexer))
        return nullopt;

    optional<TypeExpSyntax> oInnerTypeExp = ParseIdChainTypeExp(&curLexer);
    // if (!oInnerTypeExp) oInnerTypeExp = ParseFuncTypeExp(&curLexer);
    if (!oInnerTypeExp) return nullopt;

    *lexer = std::move(curLexer);
    return LocalTypeExpSyntax(std::move(*oInnerTypeExp));
}

// 
optional<TypeExpSyntax> ParseTypeExp(Lexer* lexer)
{
    if (auto oNullableTypeExp = ParseNullableTypeExp(lexer))
        return oNullableTypeExp;

    if (auto oBoxPtrTypeExp = ParseBoxPtrTypeExp(lexer))
        return oBoxPtrTypeExp;

    if (auto oLocalPtrTypeExp = ParseLocalPtrTypeExp(lexer))
        return oLocalPtrTypeExp;

    if (auto oIdChainTypeExp = ParseIdChainTypeExp(lexer))
        return oIdChainTypeExp;

    if (auto oLocalTypeExp = ParseLocalTypeExp(lexer))
        return oLocalTypeExp;

    return nullopt;
}

}