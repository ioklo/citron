#pragma once
#include "RSymbolConfig.h"

#include <variant>

namespace Citron {

class RNamespace;
class RClassDecl;
class RStructDecl;
class RImplTraitDecl;
class RDecl;

class RImplTraitDeclOuter
{
    using Variant = std::variant<RNamespace*, RClassDecl*, RStructDecl*>;
    Variant v;

public:
    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, RImplTraitDeclOuter>) && std::constructible_from<Variant, T&&>
    RImplTraitDeclOuter(T&& t) : v{std::forward<T>(t)} {}

    RSYMBOL_API void AddImplTrait(RImplTraitDecl* decl);
    RSYMBOL_API RDecl* GetDecl();
};

} // namespace Citron
