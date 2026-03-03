#pragma once

#include <optional>

namespace Citron {
struct ReExpVisitor
{
    virtual ~ReExpVisitor() {}
    virtual void Visit(ReExp_ThisVar* reExp) = 0;
    virtual void Visit(ReExp_LocalVar* reExp) = 0;
    virtual void Visit(ReExp_LocalRef* reExp) = 0;
    virtual void Visit(ReExp_LambdaVar* reExp) = 0;
    virtual void Visit(ReExp_ClassVar* reExp) = 0;
    virtual void Visit(ReExp_StructVar* reExp) = 0;
    virtual void Visit(ReExp_EnumElemVar* reExp) = 0;
    virtual void Visit(ReExp_PtrDeref* reExp) = 0;
    virtual void Visit(ReExp_BoxDeref* reExp) = 0;
    virtual void Visit(ReExp_ListIndexer* reExp) = 0;
    virtual void Visit(ReExp_Else* reExp) = 0;
};

template<class TFrom, class TVisitor>
concept ReExpConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept ReExpVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<ReExp_ThisVar*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_LocalVar*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_LocalRef*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_LambdaVar*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_ClassVar*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_StructVar*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_EnumElemVar*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_PtrDeref*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_BoxDeref*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_ListIndexer*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_Else*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;

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
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(ReExp_ThisVar* reExp) override { call(reExp); }
            void Visit(ReExp_LocalVar* reExp) override { call(reExp); }
            void Visit(ReExp_LocalRef* reExp) override { call(reExp); }
            void Visit(ReExp_LambdaVar* reExp) override { call(reExp); }
            void Visit(ReExp_ClassVar* reExp) override { call(reExp); }
            void Visit(ReExp_StructVar* reExp) override { call(reExp); }
            void Visit(ReExp_EnumElemVar* reExp) override { call(reExp); }
            void Visit(ReExp_PtrDeref* reExp) override { call(reExp); }
            void Visit(ReExp_BoxDeref* reExp) override { call(reExp); }
            void Visit(ReExp_ListIndexer* reExp) override { call(reExp); }
            void Visit(ReExp_Else* reExp) override { call(reExp); }
        };

        Bridge bridge{caller};
        reExp->Accept(bridge);
    }
    else
    {
        struct Bridge : ReExpVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(ReExp_ThisVar* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_LocalVar* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_LocalRef* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_LambdaVar* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_ClassVar* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_StructVar* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_EnumElemVar* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_PtrDeref* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_BoxDeref* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_ListIndexer* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_Else* reExp) override { result.emplace(call(reExp)); }
        };

        Bridge bridge{caller};
        reExp->Accept(bridge);
        return *bridge.result;
    }
}

} // namespace Citron