#pragma once

#include "IR0Config.h"

#include <memory>
#include <vector>
#include <Infra/Hash.h>

#include "RFuncReturn.h"
#include "RFuncParameter.h"
#include "RNames.h"

namespace Citron
{

class RNullableValueType; // struct, enum 타입 등에서 쓰일 nullable
class RNullableRefType;   // class 타입 등에서 쓰일 nullable
class RTypeVarType;  // 이것은 Symbol인가?
class RVoidType;     // builtin type
class RTupleType;    // inline type
class RFuncType;     // inline type, circular
class RLocalPtrType; // inline type
class RBoxPtrType;   // inline type
class RInstanceType; // indirect
class RDecl;
using RDeclPtr = std::shared_ptr<RDecl>;
class RMember;
using RMemberPtr = std::shared_ptr<RMember>;

class RTypeArguments;
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

class RTypeFactory;

class RTypeVisitor
{
public:
    virtual ~RTypeVisitor() { }
    virtual void Visit(RNullableValueType& type) = 0;
    virtual void Visit(RNullableRefType& type) = 0;
    virtual void Visit(RTypeVarType& type) = 0;
    virtual void Visit(RVoidType& type) = 0;
    virtual void Visit(RTupleType& type) = 0;
    virtual void Visit(RFuncType& type) = 0;
    virtual void Visit(RLocalPtrType& type) = 0;
    virtual void Visit(RBoxPtrType& type) = 0;
    virtual void Visit(RInstanceType& type) = 0;
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
    virtual ~RType() { }
    virtual RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    virtual RCustomTypeKind GetCustomTypeKind() { return RCustomTypeKind::None; }
    virtual RMemberPtr GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount);

    virtual void Accept(RTypeVisitor& visitor) = 0;
};

using RTypePtr = std::shared_ptr<RType>;

// recursive types
class RNullableValueType : public RType
{
public:
    RTypePtr innerType;

private:
    friend RTypeFactory;
    RNullableValueType(RTypePtr&& innerType);

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};

class RNullableRefType : public RType
{
public:
    RTypePtr innerType;

private:
    friend RTypeFactory;
    RNullableRefType(RTypePtr&& innerType);

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};

// trivial types
class RTypeVarType : public RType
{
public:
    int index;
    // std::string name;  다른 클래스에서도 공유할 것이므로 이름을 넣지 않는다

private:
    friend RTypeFactory;
    RTypeVarType(int index);

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};

class RVoidType : public RType
{
private:
    friend RTypeFactory;
    RVoidType();

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};

struct RTupleMemberVar
{
    RTypePtr declType;
    std::string name;

    bool operator==(const RTupleMemberVar& other) const noexcept
    {
        return declType == other.declType && name == other.name;
    }
};

class RTupleType : public RType
{
public:
    std::vector<RTupleMemberVar> memberVars;

private:
    friend RTypeFactory;
    RTupleType(std::vector<RTupleMemberVar>&& memberVars);

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};

class RFuncType : public RType
{
public:
    struct Parameter
    {
        bool bOut;
        RTypePtr type;

        Parameter(bool bOut, RTypePtr&& type);
        bool operator==(const Parameter& other) const noexcept
        {
            return bOut == other.bOut && type == other.type;
        }
    };

    bool bLocal;
    RTypePtr retType;
    std::vector<Parameter> params;

private:
    friend RTypeFactory;
    RFuncType(bool bLocal, RTypePtr&& retType, std::vector<Parameter>&& params);

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};

class RLocalPtrType : public RType
{
public:
    RTypePtr innerType;

private:
    friend RTypeFactory;
    RLocalPtrType(RTypePtr&& innerType);

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};

class RBoxPtrType : public RType
{
public:
    RTypePtr innerType;

private:
    friend RTypeFactory;
    RBoxPtrType(const RTypePtr& innerType);

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};

class RInstanceType : public RType
{
public:
    RDeclPtr decl;
    RTypeArgumentsPtr typeArgs;

private:
    friend RTypeFactory;
    RInstanceType(const RDeclPtr& decl, const RTypeArgumentsPtr& typeArgs);

public:
    RCustomTypeKind GetCustomTypeKind() override;
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

}

namespace std {

template<>
struct hash<Citron::RTupleMemberVar>
{
    size_t operator()(const Citron::RTupleMemberVar& tupleMemberVar) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, tupleMemberVar.declType);
        Citron::hash_combine(s, tupleMemberVar.name);
        return s;
    }
};

template<>
struct hash<Citron::RFuncType::Parameter>
{
    size_t operator()(const Citron::RFuncType::Parameter& parameter) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, parameter.bOut);
        Citron::hash_combine(s, parameter.type);
        return s;
    }
};

} // namespace std
