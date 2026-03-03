#pragma once
#include "RSymbolConfig.h"

#include <vector>
#include <optional>
#include <string>

#include "Infra/Hash.h"
#include "Infra/Exceptions.h"

#include "RFuncReturn.h"
#include "RFuncParameter.h"
#include "RNames.h"
#include "RDeclRes.h"

namespace Citron
{

class RTypeArguments;
class RFactory;
class RStructCtorDecl;
class RInterfaceDecl;
class RLambdaDecl;
class RTypeParamDecl;

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
    virtual RType* Apply(RTypeArguments& typeArgs) = 0;
    virtual RTypeKind GetTypeKind() = 0;
    virtual bool IsBitwiseCopyable() = 0;
    virtual std::optional<RDeclRes> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) = 0;

    virtual void Accept(RTypeVisitor& visitor) = 0;
};


// recursive types
class RType_NullableValue : public RType
{
public:
    RType* innerType;
    RFactory* factory;

private:
    friend RFactory;
    RType_NullableValue(RType* innerType, RFactory* factory);

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    bool IsBitwiseCopyable() override { return innerType->IsBitwiseCopyable(); }
    RSYMBOL_API std::optional<RDeclRes> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_NullableRef : public RType
{
public:
    RType* innerType;
    RFactory* factory;

private:
    friend RFactory;
    RType_NullableRef(RType* innerType, RFactory* factory);

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs) override;
    RTypeKind GetTypeKind() override { return innerType->GetTypeKind(); }
    bool IsBitwiseCopyable() override { return innerType->IsBitwiseCopyable(); } // 보통 reference type은 BitwiseCopyable이지만, 나중에 어떻게 될지 모르기 때문에 innerType을 따르는 것으로 한다
    RSYMBOL_API std::optional<RDeclRes> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

// trivial types
class RType_TypeVar : public RType
{
public:
    RTypeParamDecl* decl;

private:
    friend RFactory;
    RType_TypeVar(RTypeParamDecl* decl);

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs) override;
    RTypeKind GetTypeKind() override { throw NotImplementedException{}; } // TypeVar종류따라 분화할 수 있다
    bool IsBitwiseCopyable() override { throw NotImplementedException{}; } // TypeVar에 명확히 적어줘야 할 것이다
    RSYMBOL_API std::optional<RDeclRes> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_Void : public RType
{
private:
    friend RFactory;
    RType_Void();

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; } // 크기가 0인 value type
    bool IsBitwiseCopyable() override { return true; } // 크기가 0바이트이지만, 생성자, 소멸자를 따로 호출하지 않으므로 BitwiseCopyable이다
    RSYMBOL_API std::optional<RDeclRes> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
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

    // from RType
    RType* Apply(RTypeArguments& typeArgs) override { return this; } // no typeArgs
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    bool IsBitwiseCopyable() override { return true; } // 언제나 Bitwise Copyable
    std::optional<RDeclRes> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override { return std::nullopt; }

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
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    RSYMBOL_API bool IsBitwiseCopyable() override;
    RSYMBOL_API std::optional<RDeclRes> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
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
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Interface; }
    bool IsBitwiseCopyable() override { return false; } // interface 계열이므로 shared pointer로 관리될 것이므로 bitwise copyable하지 않다
    RSYMBOL_API std::optional<RDeclRes> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
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
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    bool IsBitwiseCopyable() override { return true; } // 포인터는 항상 bitwise copyable이다
    RSYMBOL_API std::optional<RDeclRes> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
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
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    bool IsBitwiseCopyable() override { return false; } // shared pointer는 생성/소멸 시점에 레퍼런스 카운팅을 할 의무가 있으므로 bitwise copyable이 아니다
    RSYMBOL_API std::optional<RDeclRes> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
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
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    bool IsBitwiseCopyable() override { return false; } // box type은 이동 생성/이동 대입만 가능하므로, Bitwise copyable이 아니다
    RSYMBOL_API std::optional<RDeclRes> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_Class : public RType
{
public:
    RClassDecl* decl;
    RTypeArguments* typeArgs;
    RFactory* factory;

private:
    friend RFactory;
    RType_Class(RClassDecl* decl, RTypeArguments* typeArgs, RFactory* factory);

public:
    RSYMBOL_API std::optional<RDeclRes_ClassVar> GetVar(const RName& name);
    RSYMBOL_API bool IsBaseOf(RType_Class& derivedClass);

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Class; }
    bool IsBitwiseCopyable() override { return false; } // class type은 내부적으로 레퍼런스 카운트로 관리되므로 bitwise copyable이 아니다
    RSYMBOL_API std::optional<RDeclRes> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_Struct : public RType
{
public:
    RStructDecl* decl;
    RTypeArguments* typeArgs;
    RFactory* factory;

private:
    friend RFactory;
    RType_Struct(RStructDecl* decl, RTypeArguments* typeArgs, RFactory* factory);

public:
    RSYMBOL_API std::optional<RDeclRes_StructVar> GetVar(const RName& name);
    RSYMBOL_API RStructCtorDecl* GetUnboundTrivialCtor();

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    bool IsBitwiseCopyable() override { return false; } // TODO: [33] struct [BitwiseCopyable] 추가
    RSYMBOL_API std::optional<RDeclRes> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_Enum : public RType
{
public:
    REnumDecl* decl;
    RTypeArguments* typeArgs;
    RFactory* factory;

private:
    friend RFactory;
    RType_Enum(REnumDecl* decl, RTypeArguments* typeArgs, RFactory* factory);

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    RSYMBOL_API bool IsBitwiseCopyable() override; // enum은 갖고 가능한 Elem의 ElemVar중 하나라도 BitwiseCopyable이 아니라면 BitwiseCopyable이 아니다
    RSYMBOL_API std::optional<RDeclRes> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_EnumElem : public RType
{
public:
    REnumElemDecl* decl;
    RTypeArguments* typeArgs;

    RFactory* factory;

private:
    friend RFactory;
    RType_EnumElem(REnumElemDecl* decl, RTypeArguments* typeArgs, RFactory* factory);

public:
    RSYMBOL_API std::optional<RDeclRes_EnumElemVar> GetVar(const RName& name);
    RSYMBOL_API RType_Enum* GetBaseEnumType();

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    RSYMBOL_API bool IsBitwiseCopyable() override;
    RSYMBOL_API std::optional<RDeclRes> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_Interface : public RType
{
public:
    RInterfaceDecl* decl;
    RTypeArguments* typeArgs;
    bool bLocal;
    RFactory* factory;

private:
    friend RFactory;
    RType_Interface(RInterfaceDecl* decl, RTypeArguments* typeArgs, bool bLocal, RFactory* factory);

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Interface; }
    bool IsBitwiseCopyable() override { return false; } // interface 계열도 shared pointer로 관리되기 때문에 bitwise copyable이 아니다
    RSYMBOL_API std::optional<RDeclRes> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

class RType_Lambda : public RType
{
public:
    RLambdaDecl* decl;
    RTypeArguments* outerTypeArgs; // 함수 자체의 typeArgs는 호출할때 binding하게 된다
    RFactory* factory;

private:
    friend RFactory;
    RType_Lambda(RLambdaDecl* decl, RTypeArguments* outerTypeArgs, RFactory* factory);

public:
    RSYMBOL_API std::vector<RFuncParameter> GetPartiallyBoundParameters(); // outerTypeArgs까지만 bound되어 있는 상태

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs) override;
    RTypeKind GetTypeKind() override { return RTypeKind::Value; }
    RSYMBOL_API bool IsBitwiseCopyable() override; // LambdaType은 캡쳐한 variable의 bitwise copyable 여부에 따라 달라진다.
    RSYMBOL_API std::optional<RDeclRes> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

    RSYMBOL_API void Accept(RTypeVisitor& visitor) override;
};

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