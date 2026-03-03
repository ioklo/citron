#pragma once

#include <optional>

namespace Citron {
struct MExpVisitor
{
    virtual ~MExpVisitor() {}
    virtual void Visit(MExp_Load* mExp) = 0;
    virtual void Visit(MExp_Store* mExp) = 0;
    virtual void Visit(MExp_Stmt* mExp) = 0;
    virtual void Visit(MExp_PtrRef* mExp) = 0;
    virtual void Visit(MExp_BoolLiteral* mExp) = 0;
    virtual void Visit(MExp_IntLiteral* mExp) = 0;
    virtual void Visit(MExp_CallIntrinsic* mExp) = 0;
    virtual void Visit(MExp_Call* mExp) = 0;
    virtual void Visit(MExp_NewStruct* mExp) = 0;
    virtual void Visit(MExp_NewEnumElem* mExp) = 0;
    virtual void Visit(MExp_NewNullableValue* mExp) = 0;
    virtual void Visit(MExp_NullableValueNullLiteral* mExp) = 0;
    virtual void Visit(MExp_Cast* mExp) = 0;
    virtual void Visit(MExp_Lambda* mExp) = 0;
    virtual void Visit(MExp_InlineBlock* mExp) = 0;
    virtual void Visit(MExp_Is* mExp) = 0;
    virtual void Visit(MExp_As* mExp) = 0;
};

template<class TFrom, class TVisitor>
concept MExpConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept MExpVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<MExp_Load*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_Store*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_Stmt*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_PtrRef*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_BoolLiteral*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_IntLiteral*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_CallIntrinsic*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_Call*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_NewStruct*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_NewEnumElem*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_NewNullableValue*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_NullableValueNullLiteral*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_Cast*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_Lambda*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_InlineBlock*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_Is*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MExp_As*>(), std::forward<TVisitorArgs>(args)...) } -> MExpConvertibleToResultType<TVisitor>;

};

template<typename TVisitor, typename... TVisitorArgs> requires MExpVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, MExp* mExp, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : MExpVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(MExp_Load* mExp) override { call(mExp); }
            void Visit(MExp_Store* mExp) override { call(mExp); }
            void Visit(MExp_Stmt* mExp) override { call(mExp); }
            void Visit(MExp_PtrRef* mExp) override { call(mExp); }
            void Visit(MExp_BoolLiteral* mExp) override { call(mExp); }
            void Visit(MExp_IntLiteral* mExp) override { call(mExp); }
            void Visit(MExp_CallIntrinsic* mExp) override { call(mExp); }
            void Visit(MExp_Call* mExp) override { call(mExp); }
            void Visit(MExp_NewStruct* mExp) override { call(mExp); }
            void Visit(MExp_NewEnumElem* mExp) override { call(mExp); }
            void Visit(MExp_NewNullableValue* mExp) override { call(mExp); }
            void Visit(MExp_NullableValueNullLiteral* mExp) override { call(mExp); }
            void Visit(MExp_Cast* mExp) override { call(mExp); }
            void Visit(MExp_Lambda* mExp) override { call(mExp); }
            void Visit(MExp_InlineBlock* mExp) override { call(mExp); }
            void Visit(MExp_Is* mExp) override { call(mExp); }
            void Visit(MExp_As* mExp) override { call(mExp); }
        };

        Bridge bridge{caller};
        mExp->Accept(bridge);
    }
    else
    {
        struct Bridge : MExpVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(MExp_Load* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_Store* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_Stmt* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_PtrRef* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_BoolLiteral* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_IntLiteral* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_CallIntrinsic* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_Call* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_NewStruct* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_NewEnumElem* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_NewNullableValue* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_NullableValueNullLiteral* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_Cast* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_Lambda* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_InlineBlock* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_Is* mExp) override { result.emplace(call(mExp)); }
            void Visit(MExp_As* mExp) override { result.emplace(call(mExp)); }
        };

        Bridge bridge{caller};
        mExp->Accept(bridge);
        return *bridge.result;
    }
}

} // namespace Citron