#pragma once
#include "RSymbolConfig.h"
#include <variant>
#include <concepts>

namespace Citron {

class RDecl;
class RTraitFuncDecl;

class RTraitMemberDecl 
{
    using Variant = std::variant<RTraitFuncDecl*>;
    Variant v;

public:
    template<typename TMemberDecl> requires (!std::same_as<TMemberDecl, RTraitMemberDecl>) && std::constructible_from<Variant, TMemberDecl&&>
    RTraitMemberDecl(TMemberDecl&& memberDecl) : v{std::forward<TMemberDecl>(memberDecl)} {}

    RSYMBOL_API RDecl* GetDecl();

    auto Visit(auto&&... args) { return std::visit(std::forward<decltype(args)>(args)..., v); }

};


} // namespace Citron