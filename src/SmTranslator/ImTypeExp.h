#pragma once
#include <variant>
#include "RSymbol/RNamespaceGroup.h"
#include "RSymbol/RAppliedDecl.h"

namespace Citron {

// class RClassDecl;
// class RStructDecl;
//class REnumDecl;
//class REnumElemDecl;
//class RInterfaceDecl;
//class RLambdaDecl;
//class RTypeParam;
class RTraitDecl;
class RType;

struct ImTypeExp_Namespaces { RNamespaceGroup namespaces; };
// struct ImTypeExp_Class { RAppliedDecl<RClassDecl> appliedDecl; };
// struct ImTypeExp_Struct { RAppliedDecl<RStructDecl> appliedDecl; };
//struct ImTypeExp_Enum { RAppliedDecl<REnumDecl> appliedDecl; };
//struct ImTypeExp_EnumElem { RAppliedDecl<REnumElemDecl> appliedDecl; };
//struct ImTypeExp_Interface { RAppliedDecl<RInterfaceDecl> appliedDecl; };
//struct ImTypeExp_Lambda { RAppliedDecl<RLambdaDecl> appliedDecl; };
//struct ImTypeExp_TypeVar { RTypeParam* decl; };
struct ImTypeExp_Trait { RAppliedDecl<RTraitDecl> appliedDecl; };
struct ImTypeExp_Type { RType* type; };

class ImTypeExp
{
    using Variant = std::variant<
        ImTypeExp_Namespaces,
        // ImTypeExp_Class,
        // ImTypeExp_Struct,
        // ImTypeExp_Enum,
        // ImTypeExp_EnumElem,
        // ImTypeExp_Interface,
        // ImTypeExp_Lambda,
        // ImTypeExp_TypeVar,
        ImTypeExp_Trait,
        ImTypeExp_Type
    >;  
    Variant v;

public:
    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, ImTypeExp>) && std::constructible_from<Variant, T&&>
    ImTypeExp(T&& res) : v{std::forward<T>(res)} {}

    template<typename... TArgs>
    auto Visit(TArgs&&... args) & { return std::visit(std::forward<TArgs>(args)..., v); }

    template<typename... TArgs>
    auto Visit(TArgs&&... args) && { return std::visit(std::forward<TArgs>(args)..., std::move(v)); }
};

} // namespace Citron