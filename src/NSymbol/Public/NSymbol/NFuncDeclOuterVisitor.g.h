#pragma once

#include <optional>

namespace Citron {
struct NFuncDeclOuterVisitor
{
    virtual ~NFuncDeclOuterVisitor() {}
    virtual void Visit(NNamespaceDecl* outer) = 0;
    virtual void Visit(NGlobalFuncDecl* outer) = 0;
    virtual void Visit(NClassDecl* outer) = 0;
    virtual void Visit(NClassCtorDecl* outer) = 0;
    virtual void Visit(NClassFuncDecl* outer) = 0;
    virtual void Visit(NStructDecl* outer) = 0;
    virtual void Visit(NStructCtorDecl* outer) = 0;
    virtual void Visit(NStructDtorDecl* outer) = 0;
    virtual void Visit(NStructFuncDecl* outer) = 0;
    virtual void Visit(NLambdaDecl* outer) = 0;
};

template<class TFrom, class TVisitor>
concept NFuncDeclOuterConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept NFuncDeclOuterVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<NNamespaceDecl*>(), std::forward<TVisitorArgs>(args)...) } -> NFuncDeclOuterConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NGlobalFuncDecl*>(), std::forward<TVisitorArgs>(args)...) } -> NFuncDeclOuterConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NClassDecl*>(), std::forward<TVisitorArgs>(args)...) } -> NFuncDeclOuterConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NClassCtorDecl*>(), std::forward<TVisitorArgs>(args)...) } -> NFuncDeclOuterConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NClassFuncDecl*>(), std::forward<TVisitorArgs>(args)...) } -> NFuncDeclOuterConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NStructDecl*>(), std::forward<TVisitorArgs>(args)...) } -> NFuncDeclOuterConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NStructCtorDecl*>(), std::forward<TVisitorArgs>(args)...) } -> NFuncDeclOuterConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NStructDtorDecl*>(), std::forward<TVisitorArgs>(args)...) } -> NFuncDeclOuterConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NStructFuncDecl*>(), std::forward<TVisitorArgs>(args)...) } -> NFuncDeclOuterConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<NLambdaDecl*>(), std::forward<TVisitorArgs>(args)...) } -> NFuncDeclOuterConvertibleToResultType<TVisitor>;

};

template<typename TVisitor, typename... TVisitorArgs> requires NFuncDeclOuterVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, NFuncDeclOuter* outer, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : NFuncDeclOuterVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(NNamespaceDecl* outer) override { call(outer); }
            void Visit(NGlobalFuncDecl* outer) override { call(outer); }
            void Visit(NClassDecl* outer) override { call(outer); }
            void Visit(NClassCtorDecl* outer) override { call(outer); }
            void Visit(NClassFuncDecl* outer) override { call(outer); }
            void Visit(NStructDecl* outer) override { call(outer); }
            void Visit(NStructCtorDecl* outer) override { call(outer); }
            void Visit(NStructDtorDecl* outer) override { call(outer); }
            void Visit(NStructFuncDecl* outer) override { call(outer); }
            void Visit(NLambdaDecl* outer) override { call(outer); }
        };

        Bridge bridge{caller};
        outer->Accept(bridge);
    }
    else
    {
        struct Bridge : NFuncDeclOuterVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(NNamespaceDecl* outer) override { result.emplace(call(outer)); }
            void Visit(NGlobalFuncDecl* outer) override { result.emplace(call(outer)); }
            void Visit(NClassDecl* outer) override { result.emplace(call(outer)); }
            void Visit(NClassCtorDecl* outer) override { result.emplace(call(outer)); }
            void Visit(NClassFuncDecl* outer) override { result.emplace(call(outer)); }
            void Visit(NStructDecl* outer) override { result.emplace(call(outer)); }
            void Visit(NStructCtorDecl* outer) override { result.emplace(call(outer)); }
            void Visit(NStructDtorDecl* outer) override { result.emplace(call(outer)); }
            void Visit(NStructFuncDecl* outer) override { result.emplace(call(outer)); }
            void Visit(NLambdaDecl* outer) override { result.emplace(call(outer)); }
        };

        Bridge bridge{caller};
        outer->Accept(bridge);
        return *bridge.result;
    }
}

} // namespace Citron