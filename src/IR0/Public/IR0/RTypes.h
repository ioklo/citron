#pragma once

#include "IR0Config.h"

#include <vector>
#include <optional>
#include <string>

#include "Infra/Hash.h"

#include "RFuncReturn.h"
#include "RFuncParameter.h"
#include "RNames.h"
#include "RMember.h"

namespace Citron
{

class RTypeArguments;
class RFactory;
class RStructCtorDecl;
class RInterfaceDecl;
class RLambdaDecl;

class RType_NullableValue;
class RType_NullableRef;
class RType_TypeVar;
class RType_Void;
class RType_Tuple;
class RType_Func;
class RType_LocalPtr;
class RType_BoxPtr;
class RType_Class;
class RType_Struct;
class RType_Enum;
class RType_EnumElem;
class RType_Interface;
class RType_Lambda;

class RTypeVisitor
{
public:
    virtual ~RTypeVisitor() = default;
    virtual void Visit(RType_NullableValue* type) = 0;
    virtual void Visit(RType_NullableRef* type) = 0;
    virtual void Visit(RType_TypeVar* type) = 0;
    virtual void Visit(RType_Void* type) = 0;
    virtual void Visit(RType_Tuple* type) = 0;
    virtual void Visit(RType_Func* type) = 0;
    virtual void Visit(RType_LocalPtr* type) = 0;
    virtual void Visit(RType_BoxPtr* type) = 0;
    virtual void Visit(RType_Class* type) = 0;
    virtual void Visit(RType_Struct* type) = 0;
    virtual void Visit(RType_Enum* type) = 0;
    virtual void Visit(RType_EnumElem* type) = 0;
    virtual void Visit(RType_Interface* type) = 0;
    virtual void Visit(RType_Lambda* type) = 0;
};

enum class RCustomTypeKind
{
    None,
    Struct,
    Class,
    Enum,
    EnumElem,
    Interface,
};

class RType
{
public:
    virtual ~RType() {}
    virtual RType* Apply(RTypeArguments& typeArgs, RFactory& factory) = 0;
    virtual RCustomTypeKind GetCustomTypeKind() { return RCustomTypeKind::None; }
    virtual std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) = 0;

    virtual void Accept(RTypeVisitor& visitor) = 0;
};


// recursive types
class RType_NullableValue : public RType
{
public:
    RType* innerType;

private:
    friend RFactory;
    RType_NullableValue(RType* innerType);

public:
    IR0_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};

class RType_NullableRef : public RType
{
public:
    RType* innerType;

private:
    friend RFactory;
    RType_NullableRef(RType* innerType);

public:
    IR0_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};

// trivial types
class RType_TypeVar : public RType
{
public:
    int index;
    // std::string name;  다른 클래스에서도 공유할 것이므로 이름을 넣지 않는다

private:
    friend RFactory;
    RType_TypeVar(int index);

public:
    IR0_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};

class RType_Void : public RType
{
private:
    friend RFactory;
    RType_Void();

public:
    IR0_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
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

private:
    friend RFactory;
    RType_Tuple(std::vector<RTupleVar>&& vars);

public:
    IR0_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};

class RType_Func : public RType
{
public:
    struct Parameter
    {
        bool bOut;
        RType* type;

        Parameter(bool bOut, RType* type);
        bool operator==(const Parameter& other) const noexcept
        {
            return bOut == other.bOut && type == other.type;
        }
    };

    bool bLocal;
    RType* retType;
    std::vector<Parameter> params;

private:
    friend RFactory;
    RType_Func(bool bLocal, RType* retType, std::vector<Parameter>&& params);

public:
    IR0_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Interface; }
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};

class RType_LocalPtr : public RType
{
public:
    RType* innerType;

private:
    friend RFactory;
    RType_LocalPtr(RType* innerType);

public:
    IR0_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};

class RType_BoxPtr : public RType
{
public:
    RType* innerType;

private:
    friend RFactory;
    RType_BoxPtr(RType* innerType);

public:
    IR0_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};

class RType_Class : public RType
{
public:
    RClassDecl* decl;
    RTypeArguments* typeArgs;

private:
    friend RFactory;
    RType_Class(RClassDecl* decl, RTypeArguments* typeArgs);

public:
    IR0_API std::optional<RMember_ClassVar> GetVar(const RName& name);
    IR0_API bool IsBaseOf(RType_Class& derivedClass);

public:
    IR0_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Class; }
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};

class RType_Struct : public RType
{
public:
    RStructDecl* decl;
    RTypeArguments* typeArgs;

private:
    friend RFactory;
    RType_Struct(RStructDecl* decl, RTypeArguments* typeArgs);

public:
    IR0_API std::optional<RMember_StructVar> GetVar(const RName& name);
    IR0_API RStructCtorDecl* GetUnboundTrivialCtor();

public:
    IR0_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Struct; }
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};

class RType_Enum : public RType
{
public:
    REnumDecl* decl;
    RTypeArguments* typeArgs;

private:
    friend RFactory;
    RType_Enum(REnumDecl* decl, RTypeArguments* typeArgs);

public:
    IR0_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Enum; }
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};

class RType_EnumElem : public RType
{
public:
    REnumElemDecl* decl;
    RTypeArguments* typeArgs;

private:
    friend RFactory;
    RType_EnumElem(REnumElemDecl* decl, RTypeArguments* typeArgs);

public:
    IR0_API std::optional<RMember_EnumElemVar> GetVar(const RName& name);
    IR0_API RType_Enum* GetBaseEnumType(RFactory& factory);

public:
    IR0_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::EnumElem; }
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};

class RType_Interface : public RType
{
public:
    RInterfaceDecl* decl;
    RTypeArguments* typeArgs;
    bool bLocal;

private:
    friend RFactory;
    RType_Interface(RInterfaceDecl* decl, RTypeArguments* typeArgs, bool bLocal);

public:
    IR0_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Interface; }
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};

class RType_Lambda : public RType
{
public:
    RLambdaDecl* decl;
    RTypeArguments* outerTypeArgs; // 함수 자체의 typeArgs는 호출할때 binding하게 된다

private:
    friend RFactory;
    RType_Lambda(RLambdaDecl* decl, RTypeArguments* outerTypeArgs);

public:
    IR0_API std::vector<RFuncParameter> GetPartiallyBoundParameters(); // outerTypeArgs까지만 bound되어 있는 상태

public:
    IR0_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Struct; }
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};

}

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
        Citron::hash_combine(s, parameter.bOut);
        Citron::hash_combine(s, parameter.type);
        return s;
    }
};

} // namespace std
