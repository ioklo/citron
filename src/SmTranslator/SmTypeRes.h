#pragma once
#include <variant>
#include "RSymbol/ROuterAppliedDecl.h"
#include "RSymbol/RNamespaceGroup.h"

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
class RType;

struct SmTypeRes_Namespaces { RNamespaceGroup namespaces; };
struct SmTypeRes_Class { ROuterAppliedDecl<RClassDecl> outerAppliedDecl; };
struct SmTypeRes_Struct { ROuterAppliedDecl<RStructDecl> outerAppliedDecl; };
struct SmTypeRes_Enum { ROuterAppliedDecl<REnumDecl> outerAppliedDecl; };
struct SmTypeRes_EnumElem { ROuterAppliedDecl<REnumElemDecl> outerAppliedDecl; };
struct SmTypeRes_Interface { ROuterAppliedDecl<RInterfaceDecl> outerAppliedDecl; };
struct SmTypeRes_Lambda { ROuterAppliedDecl<RLambdaDecl> outerAppliedDecl; };
struct SmTypeRes_TypeVar { RTypeParam* decl; };
struct SmTypeRes_Trait { ROuterAppliedDecl<RTraitDecl> outerAppliedDecl; };
struct SmTypeRes_Type { RType* type; };

// Type space Resolution Result
class SmTypeRes
{
    using Variant = std::variant<
        SmTypeRes_Namespaces,
        SmTypeRes_Class,
        SmTypeRes_Struct,
        SmTypeRes_Enum,
        SmTypeRes_EnumElem,
        SmTypeRes_Interface,
        SmTypeRes_Lambda,
        SmTypeRes_TypeVar,
        SmTypeRes_Trait
    >;
    Variant v;

public:
    template<typename T>
        requires (!std::same_as<std::remove_cvref_t<T>, SmTypeRes>) && std::constructible_from<Variant, T&&>
    SmTypeRes(T&& res) : v{std::forward<T>(res)} {}

    template<typename... TArgs>
    auto Visit(TArgs&&... args) & { return std::visit(std::forward<TArgs>(args)..., v); }

    template<typename... TArgs>
    auto Visit(TArgs&&... args) && { return std::visit(std::forward<TArgs>(args)..., std::move(v)); }

    template<typename T>
    T* GetIf() { return std::get_if<T>(&v); }
};

SmTypeRes ToSmTypeRes(RTypeArguments* outerTypeArgs, RTypeDecl* typeDecl);

} // namespace Citron