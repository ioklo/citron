#pragma once

#include "RSymbolConfig.h"

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
class RType_Primitive;
class RType_Tuple;
class RType_Func;
class RType_Ptr;
class RType_Shared;
class RType_Box;
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
    virtual void Visit(RType_Primitive* type) = 0;
    virtual void Visit(RType_Tuple* type) = 0;
    virtual void Visit(RType_Func* type) = 0;
    virtual void Visit(RType_Ptr* type) = 0;
    virtual void Visit(RType_Shared* type) = 0;
    virtual void Visit(RType_Box* type) = 0;
    virtual void Visit(RType_Class* type) = 0;
    virtual void Visit(RType_Struct* type) = 0;
    virtual void Visit(RType_Enum* type) = 0;
    virtual void Visit(RType_EnumElem* type) = 0;
    virtual void Visit(RType_Interface* type) = 0;
    virtual void Visit(RType_Lambda* type) = 0;
};

enum class RCustomTypeKind
{   
    Struct,
    Class,
    Enum,
    EnumElem,
    Interface,
    Others,
};

class RType
{
public:
    virtual ~RType() {}
    virtual RType* Apply(RTypeArguments& typeArgs, RFactory& factory) = 0;
    virtual RCustomTypeKind GetCustomTypeKind() { return RCustomTypeKind::Others; }
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
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RSYMBOL_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
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
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RSYMBOL_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
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
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RSYMBOL_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};

class RType_Void : public RType
{
private:
    friend RFactory;
    RType_Void();

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RSYMBOL_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
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
    RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override { return this; } // no typeArgs
    // RCustomTypeKind GetCustomTypeKind() { return RCustomTypeKind::Others; }
    std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override { return std::nullopt; }

    void Accept(RTypeVisitor& visitor) { visitor.Visit(this); }
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
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RSYMBOL_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
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
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Interface; }
    RSYMBOL_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};

class RType_Ptr : public RType
{
public:
    RType* innerType;

private:
    friend RFactory;
    RType_Ptr(RType* innerType);

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RSYMBOL_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};

class RType_Shared : public RType
{
public:
    RType* innerType;

private:
    friend RFactory;
    RType_Shared(RType* innerType);

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RSYMBOL_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};

class RType_Box : public RType
{
public:
    RType* innerType;

private:
    friend RFactory;
    RType_Box(RType* innerType);

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RSYMBOL_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
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
    RSYMBOL_API std::optional<RMember_ClassVar> GetVar(const RName& name);
    RSYMBOL_API bool IsBaseOf(RType_Class& derivedClass);

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Class; }
    RSYMBOL_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

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
    RSYMBOL_API std::optional<RMember_StructVar> GetVar(const RName& name);
    RSYMBOL_API RStructCtorDecl* GetUnboundTrivialCtor();

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Struct; }
    RSYMBOL_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

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
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Enum; }
    RSYMBOL_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

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
    RSYMBOL_API std::optional<RMember_EnumElemVar> GetVar(const RName& name);
    RSYMBOL_API RType_Enum* GetBaseEnumType(RFactory& factory);

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::EnumElem; }
    RSYMBOL_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;
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
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Interface; }
    RSYMBOL_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

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
    RSYMBOL_API std::vector<RFuncParameter> GetPartiallyBoundParameters(); // outerTypeArgs까지만 bound되어 있는 상태

public:
    RSYMBOL_API RType* Apply(RTypeArguments& typeArgs, RFactory& factory) override;
    RCustomTypeKind GetCustomTypeKind() override { return RCustomTypeKind::Struct; }
    RSYMBOL_API std::optional<RMember> GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount) override;

    void Accept(RTypeVisitor& visitor) override { visitor.Visit(this); }
};


template<class TFrom, class TVisitor>
concept RTypeConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept RTypeVisitable = requires(TVisitor&& v, TVisitorArgs&&... args) {
    typename std::remove_cvref_t<TVisitor>::ResultType;

    { v.Visit(std::declval<RType_NullableValue*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_NullableRef*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_TypeVar*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Void*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Primitive*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Tuple*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Func*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Ptr*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Shared*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Box*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Class*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Struct*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Enum*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_EnumElem*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Interface*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Lambda*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires RTypeVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, RType* rType, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // using TResult = decltype(v.Visit(std::declval<MNamespaceDecl*>(), std::forward<U>(u)...));

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : RTypeVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(RType_NullableValue* rType) override { call(rType); }
            void Visit(RType_NullableRef* rType) override { call(rType); }
            void Visit(RType_TypeVar* rType) override { call(rType); }
            void Visit(RType_Void* rType) override { call(rType); }
            void Visit(RType_Primitive* rType) override { call(rType); }
            void Visit(RType_Tuple* rType) override { call(rType); }
            void Visit(RType_Func* rType) override { call(rType); }
            void Visit(RType_Ptr* rType) override { call(rType); }
            void Visit(RType_Shared* rType) override { call(rType); }
            void Visit(RType_Box* rType) override { call(rType); }
            void Visit(RType_Class* rType) override { call(rType); }
            void Visit(RType_Struct* rType) override { call(rType); }
            void Visit(RType_Enum* rType) override { call(rType); }
            void Visit(RType_EnumElem* rType) override { call(rType); }
            void Visit(RType_Interface* rType) override { call(rType); }
            void Visit(RType_Lambda* rType) override { call(rType); }
        };

        Bridge bridge{caller};
        rType->Accept(bridge);
    }
    else
    {
        struct Bridge : RTypeVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(RType_NullableValue* rType) override { result.emplace(call(rType)); }
            void Visit(RType_NullableRef* rType) override { result.emplace(call(rType)); }
            void Visit(RType_TypeVar* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Void* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Primitive* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Tuple* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Func* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Ptr* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Shared* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Box* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Class* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Struct* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Enum* rType) override { result.emplace(call(rType)); }
            void Visit(RType_EnumElem* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Interface* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Lambda* rType) override { result.emplace(call(rType)); }
        };

        Bridge bridge{caller};
        rType->Accept(bridge);
        return *bridge.result;
    }
}

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
        Citron::hash_combine(s, parameter.bOut);
        Citron::hash_combine(s, parameter.type);
        return s;
    }
};

} // namespace std
