#pragma once
#include "RSymbolConfig.h"
#include <variant>

namespace Citron {

class RNamespaceDecl;
class RTypeArguments;
class RClassDecl;
class RStructDecl;
class REnumDecl;
class RTypeParam;
class RTraitDecl;
class RTypeDecl;
class REnumElemDecl;
class RInterfaceDecl;
class RLambdaDecl;

// 이름을 찾고
struct RTypeRes_Namespace { RNamespaceDecl * decl; };
struct RTypeRes_Class { RTypeArguments* outerTypeArgs; RClassDecl* decl; };
struct RTypeRes_Struct { RTypeArguments* outerTypeArgs; RStructDecl* decl; };
struct RTypeRes_Enum { RTypeArguments* outerTypeArgs; REnumDecl* decl; };
struct RTypeRes_EnumElem { RTypeArguments* outerTypeArgs; REnumElemDecl* decl; };
struct RTypeRes_Interface { RTypeArguments* outerTypeArgs; RInterfaceDecl* decl; };
struct RTypeRes_Lambda { RTypeArguments* outerTypeArgs; RLambdaDecl* decl; };
struct RTypeRes_TypeVar { RTypeParam* decl; };
struct RTypeRes_Trait { RTypeArguments* outerTypeArgs; RTraitDecl* decl; };

class RTypeRes
{
    using Variant = std::variant<
        RTypeRes_Namespace,
        RTypeRes_Class,
        RTypeRes_Struct,
        RTypeRes_Enum,
        RTypeRes_EnumElem,
        RTypeRes_Interface,
        RTypeRes_Lambda,
        RTypeRes_TypeVar,
        RTypeRes_Trait>;
    Variant v;

public:
    template<typename T>
        requires (!std::same_as<std::remove_cvref_t<T>, RTypeRes>) && std::constructible_from<Variant, T&&>
    RTypeRes(T&& res) : v{std::forward<T>(res)} {}

    template<typename... TArgs>
    auto Visit(TArgs&&... args) { return std::visit(std::forward<TArgs>(args)..., v); }

    template<typename T>
    T* GetIf() { return std::get_if<T>(&v); }
};

RSYMBOL_API RTypeRes ToRTypeRes(RTypeArguments* outerTypeArgs, RTypeDecl* typeDecl);

} // namespace Citron