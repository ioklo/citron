#pragma once

#include <memory>
#include <optional>
#include <vector>

#include "Infra/Exceptions.h"
#include "Syntax/Syntax.h"
#include "IR0/NArgument.h"

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
    std::vector<NArgument> args;
};

struct ArgumentsMatch
{
    RTypeArguments* typeArgs;
    std::vector<NArgument> args;
};

template<typename TFuncDecl>
std::optional<FuncMatch<TFuncDecl>> MatchFunc(std::vector<DeclWithOuterTypeArgs<TFuncDecl>>& items, const SArgumentsPtr& sArgs, TranslationContext& context)
{
    throw NotImplementedException();
}

std::optional<ArgumentsMatch> MatchArguments(RTypeArguments* outerTypeArgs, RTypeArguments* partialTypeArgsExceptOuter, std::vector<RFuncParameter>&& funcParams, bool bVariadic, const SArgumentsPtr& sArgs);

} // namespace SyntaxIR0Translator
} // namespace Citron

