#pragma once

#include <memory>
#include <optional>
#include <vector>

#include "Infra/Exceptions.h"
#include "Syntax/Syntax.h"
#include "MIR/MArgument.h"
#include "SExpToMExpTranslation.h"

namespace Citron {

template<typename TDecl>
struct DeclWithOuterTypeArgs;

class RTypeArguments;
struct RFuncParameter;

namespace SyntaxIR0Translator {

class TranslationContext;

template<typename TFuncDecl>
struct FuncMatch
{
    TFuncDecl* funcDecl;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;
};

struct ArgumentsMatch
{
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;
};

template<typename TFuncDecl>
std::expected<std::optional<FuncMatch<TFuncDecl>>, DiagPtr> MatchFunc(std::vector<DeclWithOuterTypeArgs<TFuncDecl>>& items, SArguments* sArgs, TranslationContext& context)
{
    // test 용 임시 구현
    if (items.size() != 1) return std::nullopt;

    std::vector<MArgument> mArgs;
    for (auto* sArgItem : sArgs->items)
    {
        auto eMExp = TranslateSExpToMExp(sArgItem->exp, /*hintType*/nullptr, context);
        RETURN_ON_ERROR(eMExp);

        mArgs.push_back(MArgument_Normal{*eMExp});
    }

    return FuncMatch<TFuncDecl>{items[0].decl, items[0].outerTypeArgs, std::move(mArgs)};
    // throw NotImplementedException{};
}

std::optional<ArgumentsMatch> MatchArguments(RTypeArguments* outerTypeArgs, RTypeArguments* partialTypeArgsExceptOuter, std::vector<RFuncParameter>&& funcParams, bool bVariadic, SArguments* sArgs);

} // namespace SyntaxIR0Translator
} // namespace Citron

