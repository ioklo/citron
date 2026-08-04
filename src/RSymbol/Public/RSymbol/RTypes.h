#pragma once
#include "RSymbolConfig.h"

#include <vector>
#include <optional>
#include <string>

#include "Infra/Hash.h"
#include "Infra/Ref.h"
#include "Infra/Exceptions.h"

#include "RCopyStrategy.h"
#include "RFuncParameter.h"
#include "RNames.h"
#include "RAppliedDecl.h"

namespace Citron {

class RTypeArguments;
class RFactory;
class RStructCtorDecl;
class RInterfaceDecl;
class RLambdaDecl;
class RTypeParam;
class RTraitDecl;
class RClassDecl;
class RStructDecl;
class REnumDecl;
class REnumElemDecl;
class RClassVarDecl;
class RStructVarDecl;
class REnumElemVarDecl;
class RDecl;

struct RTypeVisitor;

enum class RTypeKind
{
    Value,     // enum, struct, ...
    Class,     // class
    Interface, // interface
};

class RType
{
public:
    virtual ~RType() {}
    virtual RType* Apply(RTypeArguments* typeArgs) = 0;
    virtual RTypeKind GetTypeKind() = 0;
    virtual RCopyStrategy GetCopyStrategy() = 0;

    virtual void Accept(RTypeVisitor& visitor) = 0;
};

// recursive types
class RType_Nullable : public RType
{
public:
    RType* innerType;
    RFactory* factory;

private:
    friend RFactory;
    RType_Nullable(RType* innerType, RFactory* factory);

public:
    RSYMBOL_API RType* Apply(RTypeArguments* typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    RCopyStrategy GetCopyStrategy() override { return innerType->GetCopyStrategy(); }
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_NullableInplace : public RType
{
public:
    RType* innerType;
    RFactory* factory;

private:
    friend RFactory;
    RType_NullableInplace(RType* innerType, RFactory* factory);

public:
    RSYMBOL_API RType* Apply(RTypeArguments* typeArgs) override;
    RTypeKind GetTypeKind() override { return innerType->GetTypeKind(); }
    RCopyStrategy GetCopyStrategy() override { return innerType->GetCopyStrategy(); } // 보통 reference type은 BitwiseCopyable이지만, 나중에 어떻게 될지 모르기 때문에 innerType을 따르는 것으로 한다
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

// trivial types
class RType_TypeVar : public RType
{
public:
    RTypeParam* typeParam;

private:
    friend RFactory;
    RType_TypeVar(RTypeParam* typeParam);

public:
    RSYMBOL_API RType* Apply(RTypeArguments* typeArgs) override;
    RTypeKind GetTypeKind() override { throw NotImplementedException{}; } // TypeVar종류따라 분화할 수 있다
    RCopyStrategy GetCopyStrategy() override { throw NotImplementedException{}; } // TypeVar에 명확히 적어줘야 할 것이다
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_Void : public RType
{
private:
    friend RFactory;
    RType_Void();

public:
    RSYMBOL_API RType* Apply(RTypeArguments* typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; } // 크기가 0인 value type
    RCopyStrategy GetCopyStrategy() override { return RCopyStrategy::Void; }
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

enum class RType_PrimitiveKind
{
    Bool,
    Int32,
};

class RType_Primitive : public RType
{
    RType_PrimitiveKind kind;

public:
    RType_Primitive(RType_PrimitiveKind kind) : kind{kind}
    { }

    RType_PrimitiveKind GetPrimitiveKind() { return kind; }

    // from RType
    RType* Apply(RTypeArguments* typeArgs) override { return this; } // no typeArgs
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    RCopyStrategy GetCopyStrategy() override { return RCopyStrategy::Bitwise; } // 언제나 Bitwise Copyable

    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

struct RTupleVar
{
    RType* declType;
    std::string name;

    bool operator==(const RTupleVar& other) const noexcept
    {
        return declType == other.declType && name == other.name;
    }
};

class RType_Tuple : public RType
{
public:
    std::vector<RTupleVar> vars;
    RFactory* factory;

private:
    friend RFactory;
    RType_Tuple(std::vector<RTupleVar>&& vars, RFactory* factory);

public:
    RSYMBOL_API RType* Apply(RTypeArguments* typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    RSYMBOL_API RCopyStrategy GetCopyStrategy() override;
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_Func : public RType
{
public:
    struct Parameter
    {
        RFuncParameterKind kind;
        RType* type;

        Parameter(RFuncParameterKind kind, RType* type);
        bool operator==(const Parameter& other) const noexcept
        {
            return kind == other.kind && type == other.type;
        }
    };

    bool bLocal;
    RType* retType;
    std::vector<Parameter> params;

    RFactory* factory;

private:
    friend RFactory;
    RType_Func(bool bLocal, RType* retType, std::vector<Parameter>&& params, RFactory* factory);

public:
    RSYMBOL_API RType* Apply(RTypeArguments* typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Interface; }
    RCopyStrategy GetCopyStrategy() override { return RCopyStrategy::NonBitwise; } // interface 계열이므로 shared pointer로 관리될 것이므로 bitwise copyable하지 않다
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_Ptr : public RType
{
public:
    RType* innerType;
    RFactory* factory;

private:
    friend RFactory;
    RType_Ptr(RType* innerType, RFactory* factory);

public:
    RSYMBOL_API RType* Apply(RTypeArguments* typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    RCopyStrategy GetCopyStrategy() override { return RCopyStrategy::Bitwise; } // 포인터는 항상 bitwise copyable이다
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_Shared : public RType
{
public:
    RType* innerType;
    RFactory* factory;

private:
    friend RFactory;
    RType_Shared(RType* innerType, RFactory* factory);

public:
    RSYMBOL_API RType* Apply(RTypeArguments* typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    RCopyStrategy GetCopyStrategy() override { return RCopyStrategy::NonBitwise; } // shared pointer는 생성/소멸 시점에 레퍼런스 카운팅을 할 의무가 있으므로 bitwise copyable이 아니다
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_Box : public RType
{
public:
    RType* innerType;
    RFactory* factory;

private:
    friend RFactory;
    RType_Box(RType* innerType, RFactory* factory);

public:
    RSYMBOL_API RType* Apply(RTypeArguments* typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    RCopyStrategy GetCopyStrategy() override { return RCopyStrategy::NonBitwise; } // box type은 이동 생성/이동 대입만 가능하므로, Bitwise copyable이 아니다
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_Class : public RType
{
public:
    RAppliedDecl<RClassDecl> appliedDecl;
    RFactory* factory;

private:
    friend RFactory;
    RType_Class(TakeRef<RAppliedDecl<RClassDecl>> appliedDecl, RFactory* factory);

public:
    RSYMBOL_API std::optional<RAppliedDecl<RClassVarDecl>> GetVar(InRef<RName> name);
    RSYMBOL_API bool IsBaseOf(RType_Class& derivedClass);

public:
    RSYMBOL_API RType* Apply(RTypeArguments* typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Class; }
    RCopyStrategy GetCopyStrategy() override { return RCopyStrategy::NonBitwise; } // class type은 내부적으로 레퍼런스 카운트로 관리되므로 bitwise copyable이 아니다
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_Struct : public RType
{
public:
    RAppliedDecl<RStructDecl> appliedDecl;
    RFactory* factory;

private:
    friend RFactory;
    RType_Struct(TakeRef<RAppliedDecl<RStructDecl>> appliedDecl, RFactory* factory);

public:
    RSYMBOL_API std::optional<RAppliedDecl<RStructVarDecl>> GetVar(InRef<RName> name);
    RSYMBOL_API RStructCtorDecl* GetUnboundTrivialCtor();

public:
    RSYMBOL_API RType* Apply(RTypeArguments* typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    RCopyStrategy GetCopyStrategy() override { return RCopyStrategy::NonBitwise; } // TODO: [33] struct [BitwiseCopyable] 추가
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_Enum : public RType
{
public:
    RAppliedDecl<REnumDecl> appliedDecl;
    RFactory* factory;

private:
    friend RFactory;
    RType_Enum(TakeRef<RAppliedDecl<REnumDecl>> appliedDecl, RFactory* factory);

public:
    RSYMBOL_API RType* Apply(RTypeArguments* typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    RSYMBOL_API RCopyStrategy GetCopyStrategy() override; // enum은 갖고 가능한 Elem의 ElemVar중 하나라도 BitwiseCopyable이 아니라면 BitwiseCopyable이 아니다
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_EnumElem : public RType
{
public:
    RAppliedDecl<REnumElemDecl> appliedDecl;
    RFactory* factory;

private:
    friend RFactory;
    RType_EnumElem(TakeRef<RAppliedDecl<REnumElemDecl>> appliedDecl, RFactory* factory);

public:
    RSYMBOL_API std::optional<RAppliedDecl<REnumElemVarDecl>> GetVar(InRef<RName> name);
    RSYMBOL_API RType_Enum* GetEnumType();

public:
    RSYMBOL_API RType* Apply(RTypeArguments* typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    RSYMBOL_API RCopyStrategy GetCopyStrategy() override;
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_Interface : public RType
{
public:
    RAppliedDecl<RInterfaceDecl> appliedDecl;
    bool bLocal;
    RFactory* factory;

private:
    friend RFactory;
    RType_Interface(TakeRef<RAppliedDecl<RInterfaceDecl>> appliedDecl, bool bLocal, RFactory* factory);

public:
    RSYMBOL_API RType* Apply(RTypeArguments* typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Interface; }
    RCopyStrategy GetCopyStrategy() override { return RCopyStrategy::NonBitwise; } // interface 계열도 shared pointer로 관리되기 때문에 bitwise copyable이 아니다
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_Lambda : public RType
{
public:
    RAppliedDecl<RLambdaDecl> appliedDecl; // 함수 자체의 typeArgs는 호출할때 binding하게 된다
    RFactory* factory;

private:
    friend RFactory;
    RType_Lambda(RAppliedDecl<RLambdaDecl>&& appliedDecl, RFactory* factory);

public:
    RSYMBOL_API std::vector<RFuncParameter> GetPartiallyBoundParameters(); // outerTypeArgs까지만 bound되어 있는 상태

public:
    RSYMBOL_API RType* Apply(RTypeArguments* typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    RSYMBOL_API RCopyStrategy GetCopyStrategy() override; // LambdaType은 캡쳐한 variable의 bitwise copyable 여부에 따라 달라진다.
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

// some Trait 타입
class RType_Opaque : public RType
{
    struct PrivateKey {};

public:
    RAppliedDecl<RTraitDecl> appliedTrait;
    RAppliedDecl<RDecl> appliedOwnerFunc;
    RFactory* factory;

public: // emplace_back을 하려니 constructor가 public이어야 한다
    friend RFactory;
    RType_Opaque(TakeRef<RAppliedDecl<RTraitDecl>> appliedTrait, TakeRef<RAppliedDecl<RDecl>> appliedOwnerFunc, RFactory* factory, PrivateKey pk);

public: // from RType
    RSYMBOL_API RType* Apply(RTypeArguments* typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; } // opaque type은 value type으로 취급한다
    RSYMBOL_API RCopyStrategy GetCopyStrategy() override; // opaque type은 trait에 따라 bitwise copyable 여부가 달라진다
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

RSYMBOL_API RType* Apply(RType* type, RTypeArguments* typeArgs, RFactory* rFactory);

} // namespace Citron

namespace std {

template<>
struct hash<Citron::RTupleVar>
{
    size_t operator()(const Citron::RTupleVar& tupleVar) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, tupleVar.declType);
        Citron::hash_combine(s, tupleVar.name);
        return s;
    }
};

template<>
struct hash<Citron::RType_Func::Parameter>
{
    size_t operator()(const Citron::RType_Func::Parameter& parameter) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, parameter.kind);
        Citron::hash_combine(s, parameter.type);
        return s;
    }
};

} // namespace std

#include "RTypeVisitor.g.h"