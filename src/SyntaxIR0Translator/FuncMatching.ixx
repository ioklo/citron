export module Citron.SyntaxIR0Translator:FuncMatching;

import <memory>;
import <optional>;
import <vector>;

import Citron.Syntax;
import Citron.RDecls;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;

export template<typename TFuncDecl>
struct FuncMatch 
{
    std::shared_ptr<TFuncDecl> funcDecl;
    RTypeArgumentsPtr typeArgs;
    std::vector<NArgument> args;
};

export struct ArgumentsMatch
{
    RTypeArgumentsPtr typeArgs;
    std::vector<NArgument> args;
};

export template<typename TFuncDecl>
std::optional<FuncMatch<TFuncDecl>> MatchFunc(std::vector<DeclWithOuterTypeArgs<TFuncDecl>>& items, const SArgumentsPtr& sArgs, TranslationContext& context);

export std::optional<ArgumentsMatch> MatchArguments(const RTypeArgumentsPtr& outerTypeArgs, const RTypeArgumentsPtr& partialTypeArgsExceptOuter, std::vector<RFuncParameter>&& funcParams, bool bVariadic, const SArgumentsPtr& sArgs);

} // namespace Citron::SyntaxIR0Translator

