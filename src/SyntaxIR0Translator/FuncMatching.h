#pragma once

import <memory>;
import <optional>;
import <vector>;

#include <IR0/NArgument.h>

namespace Citron {

using SArgumentsPtr = std::shared_ptr<class SArguments>;

template<typename TDecl>
struct DeclWithOuterTypeArgs;

using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;

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
std::optional<FuncMatch<TFuncDecl>> MatchFunc(std::vector<DeclWithOuterTypeArgs<TFuncDecl>>& items, const SArgumentsPtr& sArgs, TranslationContext& context);

std::optional<ArgumentsMatch> MatchArguments(const RTypeArgumentsPtr& outerTypeArgs, const RTypeArgumentsPtr& partialTypeArgsExceptOuter, std::vector<RFuncParameter>&& funcParams, bool bVariadic, const SArgumentsPtr& sArgs);


} // namespace SyntaxIR0Translator
} // namespace Citron
