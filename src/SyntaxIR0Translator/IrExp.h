#pragma once

#include <optional>
#include <memory>

namespace Citron {

class RNamespaceDecl;
class RType_TypeVar;
class RClassDecl;
class RTypeArguments;
class RStructDecl;
class REnumDecl;
class RType;
class RFactory;
class RClassVarDecl;
class RStructVarDecl;
class MExp;
class MLoc;
class MSharedExp;
using MFactoryPtr = std::shared_ptr<class MFactory>;
struct TranslationContexts;

// Intermediate SharedRef Exp, 일반 ptr변환은 여기를 거치지 않도록 한다
// Syntax가 &exp 꼴일 경우 IrExp를 거쳐서 ReExp(ResolvedExp)로 변환한다

class IrExp_Namespace;
class IrExp_TypeVar;
class IrExp_Class;
class IrExp_Struct;
class IrExp_Enum;
class IrExp_ThisVar;
class IrExp_StaticRef;
class IrExp_SharedRef;
class IrExp_SharedDeref;
class IrExp_LocalValue;

class IrExpVisitor;

class IrExp
{
public:
    virtual ~IrExp() {}
    virtual void Accept(IrExpVisitor& visitor) = 0;
};

class IrExpVisitor
{
public:
    virtual ~IrExpVisitor() {}
    virtual void Visit(IrExp_Namespace* irExp) = 0;
    virtual void Visit(IrExp_TypeVar* irExp) = 0;
    virtual void Visit(IrExp_Class* irExp) = 0;
    virtual void Visit(IrExp_Struct* irExp) = 0;
    virtual void Visit(IrExp_Enum* irExp) = 0;
    virtual void Visit(IrExp_ThisVar* irExp) = 0;
    virtual void Visit(IrExp_StaticRef* irExp) = 0;
    virtual void Visit(IrExp_SharedRef* irExp) = 0;
    virtual void Visit(IrExp_SharedDeref* irExp) = 0;
    virtual void Visit(IrExp_LocalValue* irExp) = 0;
};

class IrExp_Namespace : public IrExp
{
public:
    RNamespaceDecl* decl;

public:
    IrExp_Namespace(RNamespaceDecl* decl);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(this); }
};

class IrExp_TypeVar : public IrExp
{
public:
    RType_TypeVar* type;

public:
    IrExp_TypeVar(RType_TypeVar* type);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(this); }
};

class IrExp_Class : public IrExp
{
public:
    RClassDecl* decl;
    RTypeArguments* typeArgs;

public:
    IrExp_Class(RClassDecl* decl, RTypeArguments* typeArgs);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(this); }
};

class IrExp_Struct : public IrExp
{
public:
    RStructDecl* decl;
    RTypeArguments* typeArgs;

public:
    IrExp_Struct(RStructDecl* decl, RTypeArguments* typeArgs);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(this); }
};

class IrExp_Enum : public IrExp
{
public:
    REnumDecl* decl;
    RTypeArguments* typeArgs;

public:
    IrExp_Enum(REnumDecl* decl, RTypeArguments* typeArgs);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(this); }
};

// 자체로는 invalid하지만 memberExp랑 결합되면 의미가 생기기때문에 정보를 갖고 있는다
class IrExp_ThisVar : public IrExp
{
public:
    RType* type;
public:
    IrExp_ThisVar(RType* type);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(this); }
};

// &C.x, holder없이 주소가 살아있는
class IrExp_StaticRef : public IrExp
{
public:
    MLoc* loc;

public:
    IrExp_StaticRef(MLoc* loc);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(this); }
};

class IrExp_SharedRef : public IrExp
{
public:
    MSharedExp* sharedExp;

    IrExp_SharedRef(MSharedExp* sharedExp) 
        : sharedExp{sharedExp}
    { }
    
    RType* GetTargetType();
    void Accept(IrExpVisitor& visitor) final { visitor.Visit(this); }
};

// Value로 나오는 경우
class IrExp_LocalValue : public IrExp
{
public:
    MExp* exp;

public:
    IrExp_LocalValue(MExp* exp);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(this); }
};

// *x
class IrExp_SharedDeref : public IrExp
{
public:
    MLoc* innerLoc;

public:
    IrExp_SharedDeref(MLoc* innerLoc);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(this); }
};

template<class TFrom, class TVisitor>
concept IrExpConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept IrExpVisitable = requires(TVisitor&& v, TVisitorArgs&&... args) {
    typename std::remove_cvref_t<TVisitor>::ResultType;

    { v.Visit(std::declval<IrExp_Namespace*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_TypeVar*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_Class*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_Struct*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_Enum*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_ThisVar*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_StaticRef*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_SharedRef*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_SharedDeref*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_LocalValue*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires IrExpVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, IrExp* irExp, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // using TResult = decltype(v.Visit(std::declval<MNamespaceDecl*>(), std::forward<U>(u)...));

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : IrExpVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(IrExp_Namespace* irExp) override { call(irExp); }
            void Visit(IrExp_TypeVar* irExp) override { call(irExp); }
            void Visit(IrExp_Class* irExp) override { call(irExp); }
            void Visit(IrExp_Struct* irExp) override { call(irExp); }
            void Visit(IrExp_Enum* irExp) override { call(irExp); }
            void Visit(IrExp_ThisVar* irExp) override { call(irExp); }
            void Visit(IrExp_StaticRef* irExp) override { call(irExp); }
            void Visit(IrExp_SharedRef* irExp) override { call(irExp); }
            void Visit(IrExp_SharedDeref* irExp) override { call(irExp); }
            void Visit(IrExp_LocalValue* irExp) override { call(irExp); }
        };

        Bridge bridge{caller};
        irExp->Accept(bridge);
    }
    else
    {
        struct Bridge : IrExpVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(IrExp_Namespace* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_TypeVar* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_Class* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_Struct* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_Enum* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_ThisVar* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_StaticRef* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_SharedRef* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_SharedDeref* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_LocalValue* irExp) override { result.emplace(call(irExp)); }
        };

        Bridge bridge{caller};
        irExp->Accept(bridge);
        return *bridge.result;
    }
}

} // namespace Citron