#pragma once
#include <string>
#include <vector>
#include <variant>
#include "SmAppliedDecl.h"

namespace Citron {

// RType을 실행해서 substitution된 타입을 나타낸다. SmTypeEnv하에서 의미가 있다
// RType_TypeVar(RTypeParam*)를 SmType_TypeVar(index)로 변경

class RDecl;
class RClassDecl;
class RStructDecl;
class REnumDecl;
class REnumElemDecl;
class RInterfaceDecl;
class RLambdaDecl;
class RTraitDecl;
enum class RType_PrimitiveKind;
enum class RFuncParameterKind;

struct SmType_Nullable { SmType* innerType; };
struct SmType_NullableInplace { SmType* innerType; };
struct SmType_TypeVar { size_t index; };
struct SmType_Void {};
struct SmType_Primitive { RType_PrimitiveKind kind; };
struct SmType_TupleVar { SmType* declType; std::string name; };
struct SmType_Tuple { std::vector<SmType_TupleVar> vars; };
struct SmType_Func
{
    struct Paramter { RFuncParameterKind kind; SmType* type; };

public:
    bool bLocal;
    SmType* retType;
    std::vector<Paramter> params;
};
struct SmType_Ptr { SmType* innerType; };
struct SmType_Shared { SmType* innerType; };
struct SmType_Box { SmType* innerType; };
struct SmType_Class { SmAppliedDecl<RClassDecl> appliedDecl; };
struct SmType_Struct { SmAppliedDecl<RStructDecl> appliedDecl; };
struct SmType_Enum { SmAppliedDecl<REnumDecl> appliedDecl; };
struct SmType_EnumElem { SmAppliedDecl<REnumElemDecl> appliedDecl; };
struct SmType_Interface { SmAppliedDecl<RInterfaceDecl> appliedDecl; bool bLocal; };
struct SmType_Lambda { SmAppliedDecl<RLambdaDecl> appliedDecl; };
struct SmType_Opaque { SmAppliedDecl<RTraitDecl> appliedTrait; SmAppliedDecl<RDecl> appliedOwnerFunc; };

class SmType
{
    using Variant = std::variant<
        SmType_Nullable,
        SmType_NullableInplace,
        SmType_TypeVar,
        SmType_Void,
        SmType_Primitive,
        SmType_Tuple,
        SmType_Func,
        SmType_Ptr,
        SmType_Shared,
        SmType_Box,
        SmType_Class,
        SmType_Struct,
        SmType_Enum,
        SmType_EnumElem,
        SmType_Interface,
        SmType_Lambda,
        SmType_Opaque
    >;
    Variant v;

public:
    template<typename T> requires (!std::same_as<T, SmType>) && std::constructible_from<Variant, T&&>
    SmType(T&& value) : v{std::forward<T>(value)} {}

    template<typename... TArgs>
    auto Visit(TArgs&&... args) { return std::visit(std::forward<TArgs>(args)..., v); }

    template<typename... TArgs>
    static auto Visit(SmType& x, SmType& y, TArgs&&... args) { return std::visit(std::forward<TArgs>(args)..., x.v, y.v); }
};

} // namespace Citron
