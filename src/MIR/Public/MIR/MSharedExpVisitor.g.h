#pragma once

#include <optional>

namespace Citron {
struct MSharedExpVisitor
{
    virtual ~MSharedExpVisitor() {}
    virtual void Visit(MSharedExp_Static* sharedExp) = 0;
    virtual void Visit(MSharedExp_ClassVar* sharedExp) = 0;
    virtual void Visit(MSharedExp_SharedStructVar* sharedExp) = 0;
    virtual void Visit(MSharedExp_StructVar* sharedExp) = 0;
};

template<class TFrom, class TVisitor>
concept MSharedExpConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept MSharedExpVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<MSharedExp_Static*>(), std::forward<TVisitorArgs>(args)...) } -> MSharedExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MSharedExp_ClassVar*>(), std::forward<TVisitorArgs>(args)...) } -> MSharedExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MSharedExp_SharedStructVar*>(), std::forward<TVisitorArgs>(args)...) } -> MSharedExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MSharedExp_StructVar*>(), std::forward<TVisitorArgs>(args)...) } -> MSharedExpConvertibleToResultType<TVisitor>;

};

template<typename TVisitor, typename... TVisitorArgs> requires MSharedExpVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, MSharedExp* sharedExp, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : MSharedExpVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(MSharedExp_Static* sharedExp) override { call(sharedExp); }
            void Visit(MSharedExp_ClassVar* sharedExp) override { call(sharedExp); }
            void Visit(MSharedExp_SharedStructVar* sharedExp) override { call(sharedExp); }
            void Visit(MSharedExp_StructVar* sharedExp) override { call(sharedExp); }
        };

        Bridge bridge{caller};
        sharedExp->Accept(bridge);
    }
    else
    {
        struct Bridge : MSharedExpVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(MSharedExp_Static* sharedExp) override { result.emplace(call(sharedExp)); }
            void Visit(MSharedExp_ClassVar* sharedExp) override { result.emplace(call(sharedExp)); }
            void Visit(MSharedExp_SharedStructVar* sharedExp) override { result.emplace(call(sharedExp)); }
            void Visit(MSharedExp_StructVar* sharedExp) override { result.emplace(call(sharedExp)); }
        };

        Bridge bridge{caller};
        sharedExp->Accept(bridge);
        return *bridge.result;
    }
}

} // namespace Citron