#pragma once
#include "RSymbolConfig.h"

#include <variant>

namespace Citron {

class EFuncDeclOuter;

class RDecl;
class RNamespace;
class RGlobalFuncDecl;
class RClassDecl;
class RClassCtorDecl;
class RClassFuncDecl;
class RStructDecl;
class RStructCtorDecl;
class RStructDtorDecl;
class RStructFuncDecl;
class RLambdaDecl;

class RFuncDeclOuter
{
    using Variant = std::variant<
        RNamespace*,
        RGlobalFuncDecl*,
        RClassDecl*,
        RClassCtorDecl*,
        RClassFuncDecl*,
        RStructDecl*,
        RStructCtorDecl*,
        RStructDtorDecl*,
        RStructFuncDecl*,
        RLambdaDecl*>;
    
    Variant v;

public:
    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, RFuncDeclOuter>) && std::constructible_from<Variant, T&&>
    RFuncDeclOuter(T&& funcDecl) : v{std::forward<T>(funcDecl)} {}

    RSYMBOL_API RDecl* GetRDecl();

    template<typename... TArgs>
    auto Visit(TArgs&&... args) { return std::visit(std::forward<TArgs>(args)..., v); }
};



} // namespace Citron
