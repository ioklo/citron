#pragma once
#include "RSymbolConfig.h"
#include <variant>
#include "ROuterAppliedDecl.h"
#include "RNamespaceGroup.h"

namespace Citron {

class RNamespace;
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
struct RTypeRes_Namespaces { RNamespaceGroup namespaces; };
struct RTypeRes_Class { ROuterAppliedDecl<RClassDecl> outerAppliedDecls; };
struct RTypeRes_Struct { ROuterAppliedDecl<RStructDecl> outerAppliedDecls; };
struct RTypeRes_Enum { ROuterAppliedDecl<REnumDecl> outerAppliedDecls; };
struct RTypeRes_EnumElem { ROuterAppliedDecl<REnumElemDecl> outerAppliedDecls; };
struct RTypeRes_Interface { ROuterAppliedDecl<RInterfaceDecl> outerAppliedDecls; };
struct RTypeRes_Lambda { ROuterAppliedDecl<RLambdaDecl> outerAppliedDecls; };
struct RTypeRes_TypeVar { RTypeParam* decl; };
struct RTypeRes_Trait { ROuterAppliedDecl<RTraitDecl> outerAppliedDecls; };

class RTypeRes
{
    using Variant = std::variant<
        RTypeRes_Namespaces,
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