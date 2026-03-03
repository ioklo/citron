#pragma once

#include <optional>

namespace Citron {
struct RFuncDeclVisitor
{
    virtual ~RFuncDeclVisitor() {}
    virtual void Visit(RGlobalFuncDecl* rFuncDecl) = 0;
    virtual void Visit(RClassCtorDecl* rFuncDecl) = 0;
    virtual void Visit(RClassFuncDecl* rFuncDecl) = 0;
    virtual void Visit(RStructCtorDecl* rFuncDecl) = 0;
    virtual void Visit(RStructDtorDecl* rFuncDecl) = 0;
    virtual void Visit(RStructFuncDecl* rFuncDecl) = 0;
    virtual void Visit(RLambdaDecl* rFuncDecl) = 0;
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
    { v.Visit(std::declval<RStructDtorDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RFuncDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RStructFuncDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RFuncDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RLambdaDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RFuncDeclConvertibleToResultType<TVisitor>;

};

template<typename TVisitor, typename... TVisitorArgs> requires RFuncDeclVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, RFuncDecl* rFuncDecl, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : RFuncDeclVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(RGlobalFuncDecl* rFuncDecl) override { call(rFuncDecl); }
            void Visit(RClassCtorDecl* rFuncDecl) override { call(rFuncDecl); }
            void Visit(RClassFuncDecl* rFuncDecl) override { call(rFuncDecl); }
            void Visit(RStructCtorDecl* rFuncDecl) override { call(rFuncDecl); }
            void Visit(RStructDtorDecl* rFuncDecl) override { call(rFuncDecl); }
            void Visit(RStructFuncDecl* rFuncDecl) override { call(rFuncDecl); }
            void Visit(RLambdaDecl* rFuncDecl) override { call(rFuncDecl); }
        };

        Bridge bridge{caller};
        rFuncDecl->Accept(bridge);
    }
    else
    {
        struct Bridge : RFuncDeclVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(RGlobalFuncDecl* rFuncDecl) override { result.emplace(call(rFuncDecl)); }
            void Visit(RClassCtorDecl* rFuncDecl) override { result.emplace(call(rFuncDecl)); }
            void Visit(RClassFuncDecl* rFuncDecl) override { result.emplace(call(rFuncDecl)); }
            void Visit(RStructCtorDecl* rFuncDecl) override { result.emplace(call(rFuncDecl)); }
            void Visit(RStructDtorDecl* rFuncDecl) override { result.emplace(call(rFuncDecl)); }
            void Visit(RStructFuncDecl* rFuncDecl) override { result.emplace(call(rFuncDecl)); }
            void Visit(RLambdaDecl* rFuncDecl) override { result.emplace(call(rFuncDecl)); }
        };

        Bridge bridge{caller};
        rFuncDecl->Accept(bridge);
        return *bridge.result;
    }
}

} // namespace Citron