#pragma once

#include <optional>

namespace Citron {
struct RTypeDeclVisitor
{
    virtual ~RTypeDeclVisitor() {}
    virtual void Visit(RClassDecl* rTypeDecl) = 0;
    virtual void Visit(RStructDecl* rTypeDecl) = 0;
    virtual void Visit(REnumDecl* rTypeDecl) = 0;
    virtual void Visit(REnumElemDecl* rTypeDecl) = 0;
    virtual void Visit(RInterfaceDecl* rTypeDecl) = 0;
    virtual void Visit(RLambdaDecl* rTypeDecl) = 0;
    virtual void Visit(RTypeParamDecl* rTypeDecl) = 0;
    virtual void Visit(RTraitDecl* rTypeDecl) = 0;
};

template<class TFrom, class TVisitor>
concept RTypeDeclConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept RTypeDeclVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<RClassDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RStructDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<REnumDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<REnumElemDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RInterfaceDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RLambdaDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RTypeParamDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RTraitDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeDeclConvertibleToResultType<TVisitor>;

};

template<typename TVisitor, typename... TVisitorArgs> requires RTypeDeclVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, RTypeDecl* rTypeDecl, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : RTypeDeclVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(RClassDecl* rTypeDecl) override { call(rTypeDecl); }
            void Visit(RStructDecl* rTypeDecl) override { call(rTypeDecl); }
            void Visit(REnumDecl* rTypeDecl) override { call(rTypeDecl); }
            void Visit(REnumElemDecl* rTypeDecl) override { call(rTypeDecl); }
            void Visit(RInterfaceDecl* rTypeDecl) override { call(rTypeDecl); }
            void Visit(RLambdaDecl* rTypeDecl) override { call(rTypeDecl); }
            void Visit(RTypeParamDecl* rTypeDecl) override { call(rTypeDecl); }
            void Visit(RTraitDecl* rTypeDecl) override { call(rTypeDecl); }
        };

        Bridge bridge{caller};
        rTypeDecl->Accept(bridge);
    }
    else
    {
        struct Bridge : RTypeDeclVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(RClassDecl* rTypeDecl) override { result.emplace(call(rTypeDecl)); }
            void Visit(RStructDecl* rTypeDecl) override { result.emplace(call(rTypeDecl)); }
            void Visit(REnumDecl* rTypeDecl) override { result.emplace(call(rTypeDecl)); }
            void Visit(REnumElemDecl* rTypeDecl) override { result.emplace(call(rTypeDecl)); }
            void Visit(RInterfaceDecl* rTypeDecl) override { result.emplace(call(rTypeDecl)); }
            void Visit(RLambdaDecl* rTypeDecl) override { result.emplace(call(rTypeDecl)); }
            void Visit(RTypeParamDecl* rTypeDecl) override { result.emplace(call(rTypeDecl)); }
            void Visit(RTraitDecl* rTypeDecl) override { result.emplace(call(rTypeDecl)); }
        };

        Bridge bridge{caller};
        rTypeDecl->Accept(bridge);
        return *bridge.result;
    }
}

} // namespace Citron