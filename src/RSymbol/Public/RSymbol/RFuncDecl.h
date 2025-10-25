#pragma once

#include "RSymbolConfig.h"

#include <optional>

#include "RDecl.h"
#include "RFuncReturn.h"
#include "RFuncParameter.h"

namespace Citron {

class RGlobalFuncDecl;
class RClassCtorDecl;
class RClassFuncDecl;
class RStructCtorDecl;
class RStructFuncDecl;

class RType;
class RFactory;

class RFuncDeclVisitor;

class EFuncDecl;

class RFuncDecl
{
public:
    virtual ~RFuncDecl() {}

    virtual RDecl* GetRDecl() = 0;
    virtual bool IsStatic() = 0;
    virtual size_t GetTypeParamCount() = 0;
    virtual size_t GetParamCount() = 0;
    virtual RType* GetReturnType(RTypeArguments& typeArgs, RFactory& factory) = 0;
    virtual RFuncReturn GetFuncReturn(RTypeArguments& typeArgs, RFactory& factory) = 0;
    virtual RFuncParameter GetFuncParam(RTypeArguments& typeArgs, size_t index, RFactory& factory) = 0;
    virtual void Accept(RFuncDeclVisitor& visitor) = 0;
};

class RFuncDeclVisitor
{
public:
    RSYMBOL_API virtual ~RFuncDeclVisitor() { }
    virtual void Visit(RGlobalFuncDecl* func) = 0;
    virtual void Visit(RClassCtorDecl* func) = 0;
    virtual void Visit(RClassFuncDecl* func) = 0;
    virtual void Visit(RStructCtorDecl* func) = 0;
    virtual void Visit(RStructFuncDecl* func) = 0;
    virtual void Visit(RLambdaDecl* func) = 0;
};

class REFuncDecl : public RFuncDecl
{
    EFuncDecl* funcDecl;
};

template<class TFrom, class TVisitor>
concept RFuncDeclConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept RFuncDeclVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;

    { v.Visit(std::declval<RGlobalFuncDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RFuncDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RClassCtorDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RFuncDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RClassFuncDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RFuncDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RStructCtorDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RFuncDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RStructFuncDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RFuncDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RLambdaDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RFuncDeclConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires RFuncDeclVisitable<TVisitor, TVisitorArgs...>
decltype(auto) Accept(TVisitor&& v, RFuncDecl* decl, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : RFuncDeclVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(RGlobalFuncDecl* decl) override { call(decl); }
            void Visit(RClassCtorDecl* decl) override { call(decl); }
            void Visit(RClassFuncDecl* decl) override { call(decl); }
            void Visit(RStructCtorDecl* decl) override { call(decl); }
            void Visit(RStructFuncDecl* decl) override { call(decl); }
            void Visit(RLambdaDecl* decl) override { call(decl); }
        };

        Bridge bridge{caller};
        decl->Accept(bridge);
    }
    else
    {
        struct Bridge : RFuncDeclVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(RGlobalFuncDecl* decl) override { result.emplace(call(decl)); }
            void Visit(RClassCtorDecl* decl) override { result.emplace(call(decl)); }
            void Visit(RClassFuncDecl* decl) override { result.emplace(call(decl)); }
            void Visit(RStructCtorDecl* decl) override { result.emplace(call(decl)); }
            void Visit(RStructFuncDecl* decl) override { result.emplace(call(decl)); }
            void Visit(RLambdaDecl* decl) override { result.emplace(call(decl)); }
        };

        Bridge bridge{caller};
        decl->Accept(bridge);
        return *bridge.result;
    }
}



} // namespace Citron
