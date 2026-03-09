#pragma once

#include <optional>

namespace Citron {
struct ReExpVisitor
{
    virtual ~ReExpVisitor() {}
    virtual void Visit(ReExp_Loc* reExp) = 0;
    virtual void Visit(ReExp_Exp* reExp) = 0;
    virtual void Visit(ReExp_InitExp* reExp) = 0;
};

template<class TFrom, class TVisitor>
concept ReExpConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept ReExpVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<ReExp_Loc*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_Exp*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_InitExp*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;

};

template<typename TVisitor, typename... TVisitorArgs> requires ReExpVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, ReExp* reExp, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : ReExpVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(ReExp_Loc* reExp) override { call(reExp); }
            void Visit(ReExp_Exp* reExp) override { call(reExp); }
            void Visit(ReExp_InitExp* reExp) override { call(reExp); }
        };

        Bridge bridge{caller};
        reExp->Accept(bridge);
    }
    else
    {
        struct Bridge : ReExpVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(ReExp_Loc* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_Exp* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_InitExp* reExp) override { result.emplace(call(reExp)); }
        };

        Bridge bridge{caller};
        reExp->Accept(bridge);
        return *bridge.result;
    }
}

} // namespace Citron