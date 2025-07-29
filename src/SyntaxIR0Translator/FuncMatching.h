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
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;
struct RFuncParameter;

namespace SyntaxIR0Translator {

class TranslationContext;

template<typename TFuncDecl>
struct FuncMatch
{
    std::shared_ptr<TFuncDecl> funcDecl;
    RTypeArgumentsPtr typeArgs;
    std::vector<NArgument> args;
};

struct ArgumentsMatch
{
    RTypeArgumentsPtr typeArgs;
    std::vector<NArgument> args;
};

template<typename TFuncDecl>
std::optional<FuncMatch<TFuncDecl>> MatchFunc(std::vector<DeclWithOuterTypeArgs<TFuncDecl>>& items, const SArgumentsPtr& sArgs, TranslationContext& context)
{
    throw NotImplementedException();
}

std::optional<ArgumentsMatch> MatchArguments(const RTypeArgumentsPtr& outerTypeArgs, const RTypeArgumentsPtr& partialTypeArgsExceptOuter, std::vector<RFuncParameter>&& funcParams, bool bVariadic, const SArgumentsPtr& sArgs);

} // namespace SyntaxIR0Translator
} // namespace Citron

