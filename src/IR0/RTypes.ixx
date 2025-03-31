export module Citron.RDecls:RTypes;

import "IR0Config.h";

import <memory>;
import <vector>;
import <optional>;
import <string>;

import Citron.Hash;

import :RFuncReturn;
import :RFuncParameter;
import :RNames;
import :RMember;

namespace Citron
{

export class RTypeArguments;
export using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

export class RTypeFactory;
export class RStructCtorDecl;
export class RInterfaceDecl;
export class RLambdaDecl;

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

export class RTypeVisitor
{
public:
    virtual ~RTypeVisitor() = default;
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

export enum class RCustomTypeKind
{
    None,
    Struct,
    Class,
    Enum,
    EnumElem,
    Interface,
};

export class RType
{
public:
    virtual ~RType() {}
    virtual RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    virtual RCustomTypeKind GetCustomTypeKind() { return RCustomTypeKind::None; }
    virtual std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) = 0;

    virtual void Accept(RTypeVisitor& visitor) = 0;
};

export using RTypePtr = std::shared_ptr<RType>;

// recursive types
export class RType_NullableValue : public RType
{
public:
    RTypePtr innerType;

private:
    friend RTypeFactory;
    RType_NullableValue(RTypePtr&& innerType);

public:
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

export class RType_NullableRef : public RType
{
public:
    RTypePtr innerType;

private:
    friend RTypeFactory;
    RType_NullableRef(RTypePtr&& innerType);

public:
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

// trivial types
export class RType_TypeVar : public RType
{
public:
    int index;
    // std::string name;  다른 클래스에서도 공유할 것이므로 이름을 넣지 않는다

private:
    friend RTypeFactory;
    RType_TypeVar(int index);

public:
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

export class RType_Void : public RType
{
private:
    friend RTypeFactory;
    RType_Void();

public:
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

struct RTupleVar
{
    RTypePtr declType;
    std::string name;

    bool operator==(const RTupleVar& other) const noexcept
    {
        return declType == other.declType && name == other.name;
    }
};

export class RType_Tuple : public RType
{
public:
    std::vector<RTupleVar> vars;

private:
    friend RTypeFactory;
    RType_Tuple(std::vector<RTupleVar>&& vars);

public:
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

export class RType_Func : public RType
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
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Interface; }
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

export class RType_LocalPtr : public RType
{
public:
    RTypePtr innerType;

private:
    friend RTypeFactory;
    RType_LocalPtr(RTypePtr&& innerType);

public:
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

export class RType_BoxPtr : public RType
{
public:
    RTypePtr innerType;

private:
    friend RTypeFactory;
    RType_BoxPtr(const RTypePtr& innerType);

public:
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

export class RType_Class : public RType
{
public:
    std::shared_ptr<RClassDecl> decl;
    RTypeArgumentsPtr typeArgs;

private:
    friend RTypeFactory;
    RType_Class(const std::shared_ptr<RClassDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    std::optional<RMember_ClassVar> GetVar(const RName& name);

public:
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Class; }
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

export class RType_Struct : public RType
{
public:
    std::shared_ptr<RStructDecl> decl;
    RTypeArgumentsPtr typeArgs;

private:
    friend RTypeFactory;
    RType_Struct(const std::shared_ptr<RStructDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    IR0_API std::optional<RMember_StructVar> GetVar(const RName& name);
    IR0_API std::shared_ptr<RStructCtorDecl> GetUnboundTrivialCtor();

public:
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Struct; }
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

export class RType_Enum : public RType
{
public:
    std::shared_ptr<REnumDecl> decl;
    RTypeArgumentsPtr typeArgs;

private:
    friend RTypeFactory;
    RType_Enum(const std::shared_ptr<REnumDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Enum; }
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

export class RType_EnumElem : public RType
{
public:
    std::shared_ptr<REnumElemDecl> decl;
    RTypeArgumentsPtr typeArgs;

private:
    friend RTypeFactory;
    RType_EnumElem(const std::shared_ptr<REnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    std::optional<RMember_EnumElemVar> GetVar(const RName& name);

public:
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::EnumElem; }
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

export class RType_Interface : public RType
{
public:
    std::shared_ptr<RInterfaceDecl> decl;
    RTypeArgumentsPtr typeArgs;
    bool bLocal;

private:
    friend RTypeFactory;
    RType_Interface(const std::shared_ptr<RInterfaceDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool bLocal);

public:
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Interface; }
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
};

export class RType_Lambda : public RType
{
public:
    std::shared_ptr<RLambdaDecl> decl;
    RTypeArgumentsPtr outerTypeArgs; // 함수 자체의 typeArgs는 호출할때 binding하게 된다

private:
    friend RTypeFactory;
    RType_Lambda(const std::shared_ptr<RLambdaDecl>& decl, const RTypeArgumentsPtr& outerTypeArgs);

public:
    std::vector<RFuncParameter> GetPartiallyBoundParameters(); // outerTypeArgs까지만 bound되어 있는 상태

public:
    IR0_API RTypePtr Apply(RTypeArguments& typeArgs, RTypeFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Struct; }
    IR0_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

    void Accept(RTypeVisitor& visitor) override { visitor.Visit(*this); }
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
