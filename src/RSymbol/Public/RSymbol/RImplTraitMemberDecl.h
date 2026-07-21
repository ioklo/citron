#pragma once
#include <variant>

namespace Citron {

class RImplTraitFuncDecl;

class RImplTraitMemberDecl
{
    using Variant = std::variant<RImplTraitFuncDecl*>;
    Variant v;

public:
    template<typename T> requires (!std::same_as<T, RImplTraitMemberDecl>) && std::constructible_from<Variant, T&&>
    RImplTraitMemberDecl(T&& t) : v{std::forward<T>(t)} {}

    auto Visit(auto&& visitor) { return std::visit(visitor, v); }
};


} // namespace Citron