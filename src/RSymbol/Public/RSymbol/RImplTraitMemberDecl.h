#pragma once
#include <variant>

namespace Citron {

class RImplTraitFuncDecl;

class RImplTraitMemberDecl
{
    using Variant = std::variant<RImplTraitFuncDecl*>;
    Variant v;

public:
    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, RImplTraitMemberDecl>) && std::constructible_from<Variant, T&&>
    RImplTraitMemberDecl(T&& t) : v{std::forward<T>(t)} {}

    template<typename TVisitor>
    auto Visit(TVisitor&& visitor) { return std::visit(std::forward<TVisitor>(visitor), v); }
};


} // namespace Citron