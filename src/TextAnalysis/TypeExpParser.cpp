#include "pch.h"
#include "TypeExpParser.h"

#include <optional>
#include <vector>
#include <memory>

#include <Syntax/Syntax.h>
#include <Syntax/Tokens.h>

#include "Lexer.h"
#include "ParserMisc.h"

using namespace std;

namespace Citron {

optional<vector<STypeExpPtr>> ParseTypeArgs(Lexer* lexer)
{
    vector<STypeExpPtr> typeArgs;

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

        typeArgs.push_back(std::move(oTypeArg));
    }

    *lexer = std::move(curLexer);
    return typeArgs;
}

unique_ptr<SIdTypeExp> ParseIdTypeExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oIdToken = Accept<IdentifierToken>(&curLexer);
    if (!oIdToken)
        return nullptr;

    if (auto oTypeArgs = ParseTypeArgs(&curLexer))
    {
        *lexer = std::move(curLexer);
        return make_unique<SIdTypeExp>(oIdToken->text, std::move(*oTypeArgs));
    }
    else
    {
        *lexer = std::move(curLexer);
        return make_unique<SIdTypeExp>(oIdToken->text, vector<STypeExpPtr>());
    }
}

// T?
unique_ptr<SNullableTypeExp> ParseNullableTypeExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    STypeExpPtr typeExp = ParseBoxPtrTypeExp(&curLexer);
    if (!typeExp) typeExp = ParseLocalPtrTypeExp(&curLexer);
    if (!typeExp) typeExp = ParseParenTypeExp(&curLexer);
    if (!typeExp) typeExp = ParseIdChainTypeExp(&curLexer);
    if (!typeExp) return nullptr;

    if (!Accept<QuestionToken>(&curLexer))
        return nullptr;

    *lexer = std::move(curLexer);
    return make_unique<SNullableTypeExp>(std::move(typeExp));
}

// box T*
unique_ptr<SBoxPtrTypeExp> ParseBoxPtrTypeExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<BoxToken>(&curLexer))
        return nullptr;

    STypeExpPtr typeExp = ParseParenTypeExp(&curLexer);
    if (!typeExp) typeExp = ParseIdChainTypeExp(&curLexer);
    if (!typeExp) return nullptr;

    if (!Accept<StarToken>(&curLexer))
        return nullptr;

    *lexer = std::move(curLexer);
    return make_unique<SBoxPtrTypeExp>(std::move(typeExp));
}

// T*
STypeExpPtr ParseLocalPtrTypeExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // avoid left recursion

    STypeExpPtr innerTypeExp = ParseParenTypeExp(&curLexer);
    if (!innerTypeExp) innerTypeExp = ParseIdChainTypeExp(&curLexer);
    if (!innerTypeExp) return nullptr;
    
    // 적어도 한개는 있어야 한다
    if (!Accept<StarToken>(&curLexer))
        return nullptr;
    
    STypeExpPtr curTypeExp = make_unique<SLocalPtrTypeExp>(std::move(innerTypeExp));

    while (Accept<StarToken>(&curLexer))
    {
        // NOTICE: SLocalPtrTypeExp(std::move(curTypeExp)); curTypeExp가 SLocalPtrTypeExp라면 감싸는게 아니라 이동생성자가 호출된다
        curTypeExp = make_unique<SLocalPtrTypeExp>(std::move(curTypeExp));
    }

    *lexer = std::move(curLexer);
    return curTypeExp;
}

// (T)
STypeExpPtr ParseParenTypeExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<LParenToken>(&curLexer))
        return nullptr;

    STypeExpPtr innerTypeExp = ParseNullableTypeExp(&curLexer);
    if (!innerTypeExp) innerTypeExp = ParseBoxPtrTypeExp(&curLexer);
    if (!innerTypeExp) innerTypeExp = ParseLocalPtrTypeExp(&curLexer);
    if (!innerTypeExp) innerTypeExp = ParseLocalTypeExp(&curLexer);
    if (!innerTypeExp) return nullptr;
    
    if (!Accept<RParenToken>(&curLexer))
        return nullptr;

    *lexer = std::move(curLexer);
    return innerTypeExp;
}

// ID...
STypeExpPtr ParseIdChainTypeExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto idTypeExp = ParseIdTypeExp(&curLexer);
    if (!idTypeExp)
        return nullptr;

    STypeExpPtr curTypeExp = std::move(idTypeExp);

    // .
    while (Accept<DotToken>(&curLexer))
    {
        // ID
        auto oIdToken = Accept<IdentifierToken>(&curLexer);
        if (!oIdToken)
            return nullptr;

        auto oTypeArgs = ParseTypeArgs(&curLexer);
        if (oTypeArgs)
            curTypeExp = make_unique<SMemberTypeExp>(std::move(curTypeExp), std::move(oIdToken->text), std::move(*oTypeArgs));
        else 
            curTypeExp = make_unique<SMemberTypeExp>(std::move(curTypeExp), std::move(oIdToken->text), std::vector<STypeExpPtr>{});
    }

    *lexer = std::move(curLexer);
    return curTypeExp;
}

// func<>
// std::optional<SFuncTypeExp> ParseFuncTypeExp(Lexer* lexer);

// tuple
// std::optional<STupleTypeExp> ParseTupleTypeExp(Lexer* lexer);

// local I i;
unique_ptr<SLocalTypeExp> ParseLocalTypeExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<LocalToken>(&curLexer))
        return nullptr;

    STypeExpPtr innerTypeExp = ParseIdChainTypeExp(&curLexer);
    // if (!oInnerTypeExp) oInnerTypeExp = ParseFuncTypeExp(&curLexer);
    if (!innerTypeExp) return nullptr;

    *lexer = std::move(curLexer);
    return make_unique<SLocalTypeExp>(std::move(innerTypeExp));
}

// 
STypeExpPtr ParseTypeExp(Lexer* lexer)
{
    if (auto nullableTypeExp = ParseNullableTypeExp(lexer))
        return nullableTypeExp;

    if (auto boxPtrTypeExp = ParseBoxPtrTypeExp(lexer))
        return boxPtrTypeExp;

    if (auto localPtrTypeExp = ParseLocalPtrTypeExp(lexer))
        return localPtrTypeExp;

    if (auto idChainTypeExp = ParseIdChainTypeExp(lexer))
        return idChainTypeExp;

    if (auto localTypeExp = ParseLocalTypeExp(lexer))
        return localTypeExp;

    return nullptr;
}

}