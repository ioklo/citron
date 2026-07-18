#pragma once

#include <variant>

namespace Citron {

class RNamespaceDecl;
class RTypeArguments;
class RClassDecl;
class RStructDecl;
class REnumDecl;
class RTypeParamDecl;
class RTraitDecl;

// 이름을 찾고
struct RTypeDeclRes_Namespace { RNamespaceDecl * decl; };
struct RTypeDeclRes_Class { RTypeArguments* outerTypeArgs; RClassDecl* decl; };
struct RTypeDeclRes_Struct { RTypeArguments* outerTypeArgs; RStructDecl* decl; };
struct RTypeDeclRes_Enum { RTypeArguments* outerTypeArgs; REnumDecl* decl; };
struct RTypeDeclRes_TypeVar { RTypeParamDecl* decl; };
struct RTypeDeclRes_Trait { RTypeArguments* outerTypeArgs; RTraitDecl* decl; };

class RTypeDeclRes
{
    using Variant = std::variant<
        RTypeDeclRes_Namespace,
        RTypeDeclRes_Class,
        RTypeDeclRes_Struct,
        RTypeDeclRes_Enum,
        RTypeDeclRes_TypeVar,
        RTypeDeclRes_Trait>;
    Variant v;

public:
    template<typename T>
        requires (!std::same_as<std::remove_cvref_t<T>, RTypeDeclRes>) && std::constructible_from<Variant, T&&>
    RTypeDeclRes(T&& res) : v{std::forward<T>(res)} {}

    template<typename... TArgs>
    auto Visit(TArgs&&... args) { return std::visit(std::forward<TArgs>(args)..., v); }

    template<typename T>
    T* GetIf() { return std::get_if<T>(&v); }
};

} // namespace Citron