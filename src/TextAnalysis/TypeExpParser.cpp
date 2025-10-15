#include "TypeExpParser.h"

#include <optional>
#include <vector>
#include <memory>

#include "Infra/Ptr.h"
#include "Syntax/Syntax.h"
#include "Syntax/Tokens.h"
#include "Lexer.h"
#include "ParserMisc.h"

using namespace std;

namespace Citron {

optional<vector<STypeExp*>> ParseTypeArgs(Lexer* lexer, SFactory& factory)
{
    vector<STypeExp*> typeArgs;

    Lexer curLexer = *lexer;

    if (!Accept<LessThanToken>(&curLexer))
        return nullopt;

    while (!Accept<GreaterThanToken>(&curLexer))
    {
        if (!typeArgs.empty())
            if (!Accept<CommaToken>(&curLexer))
                return nullopt;

        auto oTypeArg = ParseTypeExp(&curLexer, factory);
        if (!oTypeArg)
            return nullopt;

        typeArgs.push_back(move(oTypeArg));
    }

    *lexer = move(curLexer);
    return typeArgs;
}

STypeExp_Id* ParseIdTypeExp(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto oIdToken = Accept<IdentifierToken>(&curLexer);
    if (!oIdToken)
        return nullptr;

    if (auto oTypeArgs = ParseTypeArgs(&curLexer, factory))
    {
        *lexer = move(curLexer);
        return factory.MakeSTypeExp_Id(oIdToken->text, move(*oTypeArgs));
    }
    else
    {
        *lexer = move(curLexer);
        return factory.MakeSTypeExp_Id(oIdToken->text, vector<STypeExp*>());
    }
}

// T?
STypeExp_Nullable* ParseNullableTypeExp(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    STypeExp* typeExp = ParseBoxPtrTypeExp(&curLexer, factory);
    if (!typeExp) typeExp = ParseLocalPtrTypeExp(&curLexer, factory);
    if (!typeExp) typeExp = ParseParenTypeExp(&curLexer, factory);
    if (!typeExp) typeExp = ParseIdChainTypeExp(&curLexer, factory);
    if (!typeExp) return nullptr;

    if (!Accept<QuestionToken>(&curLexer))
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSTypeExp_Nullable(typeExp);
}

// box T*
STypeExp_BoxPtr* ParseBoxPtrTypeExp(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    if (!Accept<BoxToken>(&curLexer))
        return nullptr;

    STypeExp* typeExp = ParseParenTypeExp(&curLexer, factory);
    if (!typeExp) typeExp = ParseIdChainTypeExp(&curLexer, factory);
    if (!typeExp) return nullptr;

    if (!Accept<StarToken>(&curLexer))
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSTypeExp_BoxPtr(typeExp);
}

// T*
STypeExp* ParseLocalPtrTypeExp(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    // avoid left recursion

    STypeExp* innerTypeExp = ParseParenTypeExp(&curLexer, factory);
    if (!innerTypeExp) innerTypeExp = ParseIdChainTypeExp(&curLexer, factory);
    if (!innerTypeExp) return nullptr;
    
    // 적어도 한개는 있어야 한다
    if (!Accept<StarToken>(&curLexer))
        return nullptr;
    
    STypeExp* curTypeExp = factory.MakeSTypeExp_LocalPtr(innerTypeExp);

    while (Accept<StarToken>(&curLexer))
    {
        // NOTICE: STypeExp_LocalPtr(move(curTypeExp)); curTypeExp가 STypeExp_LocalPtr라면 감싸는게 아니라 이동생성자가 호출된다
        curTypeExp = factory.MakeSTypeExp_LocalPtr(curTypeExp);
    }

    *lexer = move(curLexer);
    return curTypeExp;
}

// (T)
STypeExp* ParseParenTypeExp(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    if (!Accept<LParenToken>(&curLexer))
        return nullptr;

    STypeExp* innerTypeExp = ParseNullableTypeExp(&curLexer, factory);
    if (!innerTypeExp) innerTypeExp = ParseBoxPtrTypeExp(&curLexer, factory);
    if (!innerTypeExp) innerTypeExp = ParseLocalPtrTypeExp(&curLexer, factory);
    if (!innerTypeExp) innerTypeExp = ParseLocalTypeExp(&curLexer, factory);
    if (!innerTypeExp) return nullptr;
    
    if (!Accept<RParenToken>(&curLexer))
        return nullptr;

    *lexer = move(curLexer);
    return innerTypeExp;
}

// ID...
STypeExp* ParseIdChainTypeExp(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto* idTypeExp = ParseIdTypeExp(&curLexer, factory);
    if (!idTypeExp)
        return nullptr;

    STypeExp* curTypeExp = idTypeExp;

    // .
    while (Accept<DotToken>(&curLexer))
    {
        // ID
        auto oIdToken = Accept<IdentifierToken>(&curLexer);
        if (!oIdToken)
            return nullptr;

        auto oTypeArgs = ParseTypeArgs(&curLexer, factory);
        if (oTypeArgs)
            curTypeExp = factory.MakeSTypeExp_Member(curTypeExp, move(oIdToken->text), move(*oTypeArgs));
        else 
            curTypeExp = factory.MakeSTypeExp_Member(curTypeExp, move(oIdToken->text), std::vector<STypeExp*>{});
    }

    *lexer = move(curLexer);
    return curTypeExp;
}

// func<>
// std::optional<SFuncTypeExp> ParseFuncTypeExp(Lexer* lexer);

// tuple
// std::optional<STupleTypeExp> ParseTupleTypeExp(Lexer* lexer);

// local I i;
STypeExp_Local* ParseLocalTypeExp(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    if (!Accept<LocalToken>(&curLexer))
        return nullptr;

    STypeExp* innerTypeExp = ParseIdChainTypeExp(&curLexer, factory);
    // if (!oInnerTypeExp) oInnerTypeExp = ParseFuncTypeExp(&curLexer);
    if (!innerTypeExp) return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSTypeExp_Local(innerTypeExp);
}

// 
STypeExp* ParseTypeExp(Lexer* lexer, SFactory& factory)
{
    if (auto* nullableTypeExp = ParseNullableTypeExp(lexer, factory))
        return nullableTypeExp;

    if (auto* boxPtrTypeExp = ParseBoxPtrTypeExp(lexer, factory))
        return boxPtrTypeExp;

    if (auto* localPtrTypeExp = ParseLocalPtrTypeExp(lexer, factory))
        return localPtrTypeExp;

    if (auto* idChainTypeExp = ParseIdChainTypeExp(lexer, factory))
        return idChainTypeExp;

    if (auto* localTypeExp = ParseLocalTypeExp(lexer, factory))
        return localTypeExp;

    return nullptr;
}

}