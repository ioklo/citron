#pragma once

#include <optional>

namespace Citron {
struct MLocVisitor
{
    virtual ~MLocVisitor() {}
    virtual void Visit(MLoc_Materialize* mLoc) = 0;
    virtual void Visit(MLoc_LocalVar* mLoc) = 0;
    virtual void Visit(MLoc_LocalRef* mLoc) = 0;
    virtual void Visit(MLoc_LambdaVar* mLoc) = 0;
    virtual void Visit(MLoc_ListIndexer* mLoc) = 0;
    virtual void Visit(MLoc_StructVar* mLoc) = 0;
    virtual void Visit(MLoc_ClassVar* mLoc) = 0;
    virtual void Visit(MLoc_EnumElemVar* mLoc) = 0;
    virtual void Visit(MLoc_This* mLoc) = 0;
    virtual void Visit(MLoc_PtrDeref* mLoc) = 0;
    virtual void Visit(MLoc_SharedDeref* mLoc) = 0;
    virtual void Visit(MLoc_NullableValue* mLoc) = 0;
};

template<class TFrom, class TVisitor>
concept MLocConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept MLocVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<MLoc_Materialize*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_LocalVar*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_LocalRef*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_LambdaVar*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_ListIndexer*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_StructVar*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_ClassVar*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_EnumElemVar*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_This*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_PtrDeref*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_SharedDeref*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_NullableValue*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;

};

template<typename TVisitor, typename... TVisitorArgs> requires MLocVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, MLoc* mLoc, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : MLocVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(MLoc_Materialize* mLoc) override { call(mLoc); }
            void Visit(MLoc_LocalVar* mLoc) override { call(mLoc); }
            void Visit(MLoc_LocalRef* mLoc) override { call(mLoc); }
            void Visit(MLoc_LambdaVar* mLoc) override { call(mLoc); }
            void Visit(MLoc_ListIndexer* mLoc) override { call(mLoc); }
            void Visit(MLoc_StructVar* mLoc) override { call(mLoc); }
            void Visit(MLoc_ClassVar* mLoc) override { call(mLoc); }
            void Visit(MLoc_EnumElemVar* mLoc) override { call(mLoc); }
            void Visit(MLoc_This* mLoc) override { call(mLoc); }
            void Visit(MLoc_PtrDeref* mLoc) override { call(mLoc); }
            void Visit(MLoc_SharedDeref* mLoc) override { call(mLoc); }
            void Visit(MLoc_NullableValue* mLoc) override { call(mLoc); }
        };

        Bridge bridge{caller};
        mLoc->Accept(bridge);
    }
    else
    {
        struct Bridge : MLocVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(MLoc_Materialize* mLoc) override { result.emplace(call(mLoc)); }
            void Visit(MLoc_LocalVar* mLoc) override { result.emplace(call(mLoc)); }
            void Visit(MLoc_LocalRef* mLoc) override { result.emplace(call(mLoc)); }
            void Visit(MLoc_LambdaVar* mLoc) override { result.emplace(call(mLoc)); }
            void Visit(MLoc_ListIndexer* mLoc) override { result.emplace(call(mLoc)); }
            void Visit(MLoc_StructVar* mLoc) override { result.emplace(call(mLoc)); }
            void Visit(MLoc_ClassVar* mLoc) override { result.emplace(call(mLoc)); }
            void Visit(MLoc_EnumElemVar* mLoc) override { result.emplace(call(mLoc)); }
            void Visit(MLoc_This* mLoc) override { result.emplace(call(mLoc)); }
            void Visit(MLoc_PtrDeref* mLoc) override { result.emplace(call(mLoc)); }
            void Visit(MLoc_SharedDeref* mLoc) override { result.emplace(call(mLoc)); }
            void Visit(MLoc_NullableValue* mLoc) override { result.emplace(call(mLoc)); }
        };

        Bridge bridge{caller};
        mLoc->Accept(bridge);
        return *bridge.result;
    }
}

} // namespace Citron