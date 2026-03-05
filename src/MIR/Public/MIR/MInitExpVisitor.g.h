#pragma once

#include <optional>

namespace Citron {
struct MInitExpVisitor
{
    virtual ~MInitExpVisitor() {}
    virtual void Visit(MInitExp_Shared* mInitExp) = 0;
    virtual void Visit(MInitExp_SharedRef* mInitExp) = 0;
    virtual void Visit(MInitExp_Stmt* mInitExp) = 0;
    virtual void Visit(MInitExp_String* mInitExp) = 0;
    virtual void Visit(MInitExp_List* mInitExp) = 0;
    virtual void Visit(MInitExp_CallIntrinsic* mInitExp) = 0;
    virtual void Visit(MInitExp_NewClass* mInitExp) = 0;
    virtual void Visit(MInitExp_StructCtor* mInitExp) = 0;
    virtual void Visit(MInitExp_Call* mInitExp) = 0;
    virtual void Visit(MInitExp_NewEnumElem* mInitExp) = 0;
    virtual void Visit(MInitExp_NewNullableValue* mInitExp) = 0;
    virtual void Visit(MInitExp_NullableValueNullLiteral* mInitExp) = 0;
    virtual void Visit(MInitExp_NullableRefNullLiteral* mInitExp) = 0;
    virtual void Visit(MInitExp_Cast* mInitExp) = 0;
    virtual void Visit(MInitExp_Lambda* mInitExp) = 0;
    virtual void Visit(MInitExp_InlineBlock* mInitExp) = 0;
    virtual void Visit(MInitExp_As* mInitExp) = 0;
};

template<class TFrom, class TVisitor>
concept MInitExpConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept MInitExpVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<MInitExp_Shared*>(), std::forward<TVisitorArgs>(args)...) } -> MInitExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MInitExp_SharedRef*>(), std::forward<TVisitorArgs>(args)...) } -> MInitExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MInitExp_Stmt*>(), std::forward<TVisitorArgs>(args)...) } -> MInitExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MInitExp_String*>(), std::forward<TVisitorArgs>(args)...) } -> MInitExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MInitExp_List*>(), std::forward<TVisitorArgs>(args)...) } -> MInitExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MInitExp_CallIntrinsic*>(), std::forward<TVisitorArgs>(args)...) } -> MInitExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MInitExp_NewClass*>(), std::forward<TVisitorArgs>(args)...) } -> MInitExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MInitExp_StructCtor*>(), std::forward<TVisitorArgs>(args)...) } -> MInitExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MInitExp_Call*>(), std::forward<TVisitorArgs>(args)...) } -> MInitExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MInitExp_NewEnumElem*>(), std::forward<TVisitorArgs>(args)...) } -> MInitExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MInitExp_NewNullableValue*>(), std::forward<TVisitorArgs>(args)...) } -> MInitExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MInitExp_NullableValueNullLiteral*>(), std::forward<TVisitorArgs>(args)...) } -> MInitExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MInitExp_NullableRefNullLiteral*>(), std::forward<TVisitorArgs>(args)...) } -> MInitExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MInitExp_Cast*>(), std::forward<TVisitorArgs>(args)...) } -> MInitExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MInitExp_Lambda*>(), std::forward<TVisitorArgs>(args)...) } -> MInitExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MInitExp_InlineBlock*>(), std::forward<TVisitorArgs>(args)...) } -> MInitExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MInitExp_As*>(), std::forward<TVisitorArgs>(args)...) } -> MInitExpConvertibleToResultType<TVisitor>;

};

template<typename TVisitor, typename... TVisitorArgs> requires MInitExpVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, MInitExp* mInitExp, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : MInitExpVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(MInitExp_Shared* mInitExp) override { call(mInitExp); }
            void Visit(MInitExp_SharedRef* mInitExp) override { call(mInitExp); }
            void Visit(MInitExp_Stmt* mInitExp) override { call(mInitExp); }
            void Visit(MInitExp_String* mInitExp) override { call(mInitExp); }
            void Visit(MInitExp_List* mInitExp) override { call(mInitExp); }
            void Visit(MInitExp_CallIntrinsic* mInitExp) override { call(mInitExp); }
            void Visit(MInitExp_NewClass* mInitExp) override { call(mInitExp); }
            void Visit(MInitExp_StructCtor* mInitExp) override { call(mInitExp); }
            void Visit(MInitExp_Call* mInitExp) override { call(mInitExp); }
            void Visit(MInitExp_NewEnumElem* mInitExp) override { call(mInitExp); }
            void Visit(MInitExp_NewNullableValue* mInitExp) override { call(mInitExp); }
            void Visit(MInitExp_NullableValueNullLiteral* mInitExp) override { call(mInitExp); }
            void Visit(MInitExp_NullableRefNullLiteral* mInitExp) override { call(mInitExp); }
            void Visit(MInitExp_Cast* mInitExp) override { call(mInitExp); }
            void Visit(MInitExp_Lambda* mInitExp) override { call(mInitExp); }
            void Visit(MInitExp_InlineBlock* mInitExp) override { call(mInitExp); }
            void Visit(MInitExp_As* mInitExp) override { call(mInitExp); }
        };

        Bridge bridge{caller};
        mInitExp->Accept(bridge);
    }
    else
    {
        struct Bridge : MInitExpVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(MInitExp_Shared* mInitExp) override { result.emplace(call(mInitExp)); }
            void Visit(MInitExp_SharedRef* mInitExp) override { result.emplace(call(mInitExp)); }
            void Visit(MInitExp_Stmt* mInitExp) override { result.emplace(call(mInitExp)); }
            void Visit(MInitExp_String* mInitExp) override { result.emplace(call(mInitExp)); }
            void Visit(MInitExp_List* mInitExp) override { result.emplace(call(mInitExp)); }
            void Visit(MInitExp_CallIntrinsic* mInitExp) override { result.emplace(call(mInitExp)); }
            void Visit(MInitExp_NewClass* mInitExp) override { result.emplace(call(mInitExp)); }
            void Visit(MInitExp_StructCtor* mInitExp) override { result.emplace(call(mInitExp)); }
            void Visit(MInitExp_Call* mInitExp) override { result.emplace(call(mInitExp)); }
            void Visit(MInitExp_NewEnumElem* mInitExp) override { result.emplace(call(mInitExp)); }
            void Visit(MInitExp_NewNullableValue* mInitExp) override { result.emplace(call(mInitExp)); }
            void Visit(MInitExp_NullableValueNullLiteral* mInitExp) override { result.emplace(call(mInitExp)); }
            void Visit(MInitExp_NullableRefNullLiteral* mInitExp) override { result.emplace(call(mInitExp)); }
            void Visit(MInitExp_Cast* mInitExp) override { result.emplace(call(mInitExp)); }
            void Visit(MInitExp_Lambda* mInitExp) override { result.emplace(call(mInitExp)); }
            void Visit(MInitExp_InlineBlock* mInitExp) override { result.emplace(call(mInitExp)); }
            void Visit(MInitExp_As* mInitExp) override { result.emplace(call(mInitExp)); }
        };

        Bridge bridge{caller};
        mInitExp->Accept(bridge);
        return *bridge.result;
    }
}

} // namespace Citron