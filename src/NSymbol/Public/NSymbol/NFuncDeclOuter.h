#pragma once
#include "NSymbolConfig.h"
#include <variant>

namespace Citron {

class NDecl;
class NNamespaceDecl;
class NGlobalFuncDecl;
class NClassDecl;
class NClassCtorDecl;
class NClassFuncDecl;
class NStructDecl;
class NStructCtorDecl;
class NStructDtorDecl;
class NStructFuncDecl;
class NLambdaDecl;

class NFuncDeclOuter
{
    using Variant = std::variant<
        NNamespaceDecl*,
        NGlobalFuncDecl*,
        NClassDecl*,
        NClassCtorDecl*,
        NClassFuncDecl*,
        NStructDecl*,
        NStructCtorDecl*,
        NStructDtorDecl*,
        NStructFuncDecl*,
        NLambdaDecl*>;

    Variant v;

public:
    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, NFuncDeclOuter>) && std::constructible_from<Variant, T&&>
    NFuncDeclOuter(T&& outer) : v{std::forward<T>(outer)} {}

    NSYMBOL_API NDecl* GetNDecl();

    template<typename... TArgs>
    auto Visit(TArgs&&... args) { return std::visit(std::forward<TArgs>(args)..., v); }
};

}