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
using MFactoryPtr = std::shared_ptr<class MFactory>;
struct TranslationContexts;

// Intermediate SharedRef Exp, 일반 ptr변환은 여기를 거치지 않도록 한다
// Syntax가 &exp 꼴일 경우 IrExp를 거쳐서 ReExp(ResolvedExp)로 변환한다

class IrExp_Namespace;
class IrExp_Class;
class IrExp_Struct;
class IrExp_Static;
class IrExp_ClassVar;
class IrExp_SharedStructVar;
class IrExp_StructVar;
class IrExp_Deref;
class IrExp_Exp;
class IrExp_Loc;

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
    virtual void Visit(IrExp_Class* irExp) = 0;
    virtual void Visit(IrExp_Struct* irExp) = 0;
    virtual void Visit(IrExp_Static* irExp) = 0;
    virtual void Visit(IrExp_ClassVar* irExp) = 0;
    virtual void Visit(IrExp_SharedStructVar* irExp) = 0;
    virtual void Visit(IrExp_StructVar* irExp) = 0;
    virtual void Visit(IrExp_Deref* irExp) = 0;
    virtual void Visit(IrExp_Exp* irExp) = 0;
    virtual void Visit(IrExp_Loc* irExp) = 0;
};

class IrExp_Namespace : public IrExp
{
public:
    RNamespaceDecl* decl;

public:
    IrExp_Namespace(RNamespaceDecl* decl);
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

// &C.x
class IrExp_Static : public IrExp
{
    RFactoryPtr rFactory;
public:
    MLoc* loc;

public:
    IrExp_Static(MLoc* loc, const RFactoryPtr& rFactory)
        : loc{loc}, rFactory{rFactory}
    { }
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(this); }
};

// &c.x => IrExp_ClassVar(MLoc_LocalVar("c"), C::x)
class IrExp_ClassVar : public IrExp
{
    RFactoryPtr rFactory;
public:
    MLoc* base;
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    IrExp_ClassVar(MLoc* base, RClassVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory)
        : base{base}, decl{decl}, typeArgs{typeArgs}, rFactory{rFactory}
    { }

    void Accept(IrExpVisitor& visitor) override { visitor.Visit(this); }
};

// shared S pS;
// &ps->x => IrExp_SharedStructVar(MLoc_LocalVar("pS"), S::x)
// IrExp_SharedStructVar
class IrExp_SharedStructVar : public IrExp
{
    RFactoryPtr rFactory;
public:
    MLoc* base;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    IrExp_SharedStructVar(MLoc* base, RStructVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory)
        : base{base}, decl{decl}, typeArgs{typeArgs}, rFactory{rFactory}
    { }

    void Accept(IrExpVisitor& visitor) override { visitor.Visit(this); }
};

// C c;
// shared A a = &c.s.a; => IrExp_StructVar(IrExp_ClassVar(MLoc_LocalVar("c"), C::s), A::a)
class IrExp_StructVar : public IrExp
{
public:
    IrExp* base;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;

private:
    RFactoryPtr rFactory;

public:
    IrExp_StructVar(IrExp* base, RStructVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory)
        : base{base}, decl{decl}, typeArgs{typeArgs}, rFactory{rFactory}
    { }
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(this); }
};

// (*pS).id 를 처리하기 위해서
// *x 모양을 따로 들고 있는다. IrExp_Loc{MLoc_Deref}는 만들어지면 안된다
class IrExp_Deref : public IrExp
{
public:
    MLoc* innerLoc;

public:
    IrExp_Deref(MLoc* innerLoc)
        : innerLoc{innerLoc}
    { }
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(this); }
};

// exp로 나오는 경우
class IrExp_Exp : public IrExp
{
public:
    MExp* exp;

public:
    IrExp_Exp(MExp* exp);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(this); }
};

class IrExp_Loc : public IrExp
{
public:
    MLoc* loc;

public:
    IrExp_Loc(MLoc* loc);
    void Accept(IrExpVisitor& visitor) override { visitor.Visit(this); }
};

template<class TFrom, class TVisitor>
concept IrExpConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept IrExpVisitable = requires(TVisitor&& v, TVisitorArgs&&... args) {
    typename std::remove_cvref_t<TVisitor>::ResultType;

    { v.Visit(std::declval<IrExp_Namespace*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_Class*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_Struct*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_Static*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_ClassVar*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_SharedStructVar*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_StructVar*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_Deref*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_Exp*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_Loc*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
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
            void Visit(IrExp_Class* irExp) override { call(irExp); }
            void Visit(IrExp_Struct* irExp) override { call(irExp); }
            void Visit(IrExp_Static* irExp) override { call(irExp); }
            void Visit(IrExp_ClassVar* irExp) override { call(irExp); }
            void Visit(IrExp_SharedStructVar* irExp) override { call(irExp); }
            void Visit(IrExp_StructVar* irExp) override { call(irExp); }
            void Visit(IrExp_Deref* irExp) override { call(irExp); }
            void Visit(IrExp_Exp* irExp) override { call(irExp); }
            void Visit(IrExp_Loc* irExp) override { call(irExp); }
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
            void Visit(IrExp_Class* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_Struct* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_Static* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_ClassVar* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_SharedStructVar* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_StructVar* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_Deref* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_Exp* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_Loc* irExp) override { result.emplace(call(irExp)); }
        };

        Bridge bridge{caller};
        irExp->Accept(bridge);
        return *bridge.result;
    }
}

} // namespace Citron