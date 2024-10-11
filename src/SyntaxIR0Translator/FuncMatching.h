#pragma once

#include <memory>
#include <optional>
#include <vector>

#include <IR0/RArgument.h>

namespace Citron {


using SArgumentsPtr = std::shared_ptr<class SArguments>;

template<typename TDecl>
struct RDeclWithOuterTypeArgs;

using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;

struct RFuncParameter;

namespace SyntaxIR0Translator {

using ScopeContextPtr = std::shared_ptr<class ScopeContext>;

template<typename TFuncDecl>
struct FuncMatch 
{
    std::shared_ptr<TFuncDecl> funcDecl;
    RTypeArgumentsPtr typeArgs;
    std::vector<RArgument> args;
};

struct ArgumentsMatch
{
    RTypeArgumentsPtr typeArgs;
    std::vector<RArgument> args;
};


template<typename TFuncDecl>
std::optional<FuncMatch<TFuncDecl>> MatchFunc(std::vector<RDeclWithOuterTypeArgs<TFuncDecl>>& items, const SArgumentsPtr& sArgs, const ScopeContextPtr& context);

std::optional<ArgumentsMatch> MatchArguments(const RTypeArgumentsPtr& outerTypeArgs, const RTypeArgumentsPtr& partialTypeArgsExceptOuter, std::vector<RFuncParameter>&& funcParams, bool bVariadic, const SArgumentsPtr& sArgs);


} // namespace SyntaxIR0Translator
} // namespace Citron
