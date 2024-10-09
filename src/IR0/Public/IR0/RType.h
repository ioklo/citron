#pragma once

#include "IR0Config.h"

#include <memory>
#include <vector>
#include <optional>
#include <Infra/Hash.h>

#include "RFuncReturn.h"
#include "RFuncParameter.h"
#include "RNames.h"
#include "RMember.h"

namespace Citron
{

class RType_NullableValue; // struct, enum 타입 등에서 쓰일 nullable
class RType_NullableRef;   // class 타입 등에서 쓰일 nullable
class RType_TypeVar;  // 이것은 Symbol인가?
class RType_Void;     // builtin type
class RType_Tuple;    // inline type
class RType_Func;     // inline type, circular
class RType_LocalPtr; // inline type
class RType_BoxPtr;   // inline type

class RType_Class;    // custom type
class RType_Struct;
class RType_Enum;
class RType_EnumElem;
class RType_Interface;
class RType_Lambda;

class RClassDecl;
class RStructDecl;
class REnumDecl;
class REnumElemDecl;
class RInterfaceDecl;
class RLambdaDecl;

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
    virtual void Visit(RType_NullableValue& type) = 0;
    virtual void Visit(RType_NullableRef& type) = 0;
    virtual void Visit(RType_TypeVar& type) = 0;
    virtual void Visit(RType_Void& type) = 0;
    virtual void Visit(RType_Tuple& type) = 0;
    virtual void Visit(RType_Func& type) = 0;
    virtual void Visit(RType_LocalPtr& type) = 0;
    virtual void Visit(RType_BoxPtr& type) = 0;
    virtual void Visit(RType_Class& type) = 0;
    virtual void Visit(RType_Struct& type) = 0;
    virtual void Visit(RType_Enum& type) = 0;
    virtual void Visit(RType_EnumElem& type) = 0;
    virtual void Visit(RType_Interface& type) = 0;
    virtual void Visit(RType_Lambda& type) = 0;
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
class RType_NullableValue : public RType
{
public:
    RTypePtr innerType;

private:
    friend RTypeFactory;
    RType_NullableValue(RTypePtr&& innerType);

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};

class RType_NullableRef : public RType
{
public:
    RTypePtr innerType;

private:
    friend RTypeFactory;
    RType_NullableRef(RTypePtr&& innerType);

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};

// trivial types
class RType_TypeVar : public RType
{
public:
    int index;
    // std::string name;  다른 클래스에서도 공유할 것이므로 이름을 넣지 않는다

private:
    friend RTypeFactory;
    RType_TypeVar(int index);

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};

class RType_Void : public RType
{
private:
    friend RTypeFactory;
    RType_Void();

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

class RType_Tuple : public RType
{
public:
    std::vector<RTupleMemberVar> memberVars;

private:
    friend RTypeFactory;
    RType_Tuple(std::vector<RTupleMemberVar>&& memberVars);

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};

class RType_Func : public RType
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
    RType_Func(bool bLocal, RTypePtr&& retType, std::vector<Parameter>&& params);

public:
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Interface; }
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};

class RType_LocalPtr : public RType
{
public:
    RTypePtr innerType;

private:
    friend RTypeFactory;
    RType_LocalPtr(RTypePtr&& innerType);

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};

class RType_BoxPtr : public RType
{
public:
    RTypePtr innerType;

private:
    friend RTypeFactory;
    RType_BoxPtr(const RTypePtr& innerType);

public:
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};

class RType_Class : public RType
{
public:
    std::shared_ptr<RClassDecl> decl;
    RTypeArgumentsPtr typeArgs;

private:
    friend RTypeFactory;
    RType_Class(const std::shared_ptr<RClassDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    std::optional<RMember_ClassMemberVar> GetMemberVar(const RName& name);

    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Class; }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

class RType_Struct : public RType
{
public:
    std::shared_ptr<RStructDecl> decl;
    RTypeArgumentsPtr typeArgs;

private:
    friend RTypeFactory;
    RType_Struct(const std::shared_ptr<RStructDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    std::optional<RMember_StructMemberVar> GetMemberVar(const RName& name);
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Struct; }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

class RType_Enum : public RType
{
public:
    std::shared_ptr<REnumDecl> decl;
    RTypeArgumentsPtr typeArgs;

private:
    friend RTypeFactory;
    RType_Enum(const std::shared_ptr<REnumDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Enum; }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

class RType_EnumElem : public RType
{
public:
    std::shared_ptr<REnumElemDecl> decl;
    RTypeArgumentsPtr typeArgs;

private:
    friend RTypeFactory;
    RType_EnumElem(const std::shared_ptr<REnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    std::optional<RMember_EnumElemMemberVar> GetMemberVar(const RName& name);

    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::EnumElem; }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

class RType_Interface : public RType
{
public:
    std::shared_ptr<RInterfaceDecl> decl;
    RTypeArgumentsPtr typeArgs;

private:
    friend RTypeFactory;
    RType_Interface(const std::shared_ptr<RInterfaceDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Interface; }
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

class RType_Lambda : public RType
{
public:
    std::shared_ptr<RLambdaDecl> decl;
    RTypeArgumentsPtr typeArgs;

private:
    friend RTypeFactory;
    RType_Lambda(const std::shared_ptr<RLambdaDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Struct; }
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
