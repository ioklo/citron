#pragma once
#include "RSymbolConfig.h"
#include <variant>
#include <concepts>

namespace Citron {

class RDecl;
class RNamespaceDecl;
class RClassDecl;
class RStructDecl;
class RTypeDecl;

class RTypeDeclOuterVisitor;

enum class RNamespaceMemberAccessor;
enum class RClassMemberAccessor;
enum class RStructMemberAccessor;

struct RTypeDeclOuter_Namespace { RNamespaceDecl* decl; RNamespaceMemberAccessor accessor; };
struct RTypeDeclOuter_Class { RClassDecl* decl; RClassMemberAccessor accessor; };
struct RTypeDeclOuter_Struct { RStructDecl* decl; RStructMemberAccessor accessor; };

// RTypeDeclOuter with accessor
class RTypeDeclOuter
{
    using Variant = std::variant<
        RTypeDeclOuter_Namespace,
        RTypeDeclOuter_Class,
        RTypeDeclOuter_Struct>;

    Variant v;

public:
    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, RTypeDeclOuter>)
    RTypeDeclOuter(T&& t) : v{std::forward<T>(t)}
    {
    }

    RSYMBOL_API RDecl* GetDecl();

    RSYMBOL_API void AddType(RTypeDecl* typeDecl);
};

} // namespace Citron