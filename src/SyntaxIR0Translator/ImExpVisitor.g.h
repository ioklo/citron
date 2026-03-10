#pragma once

#include <optional>

namespace Citron {
struct ImExpVisitor
{
    virtual ~ImExpVisitor() {}
    virtual void Visit(ImExp_Namespace* imExp) = 0;
    virtual void Visit(ImExp_GlobalFuncs* imExp) = 0;
    virtual void Visit(ImExp_TypeVar* imExp) = 0;
    virtual void Visit(ImExp_Class* imExp) = 0;
    virtual void Visit(ImExp_ClassFuncs* imExp) = 0;
    virtual void Visit(ImExp_Struct* imExp) = 0;
    virtual void Visit(ImExp_StructFuncs* imExp) = 0;
    virtual void Visit(ImExp_Enum* imExp) = 0;
    virtual void Visit(ImExp_EnumElem* imExp) = 0;
    virtual void Visit(ImExp_ClassVar* imExp) = 0;
    virtual void Visit(ImExp_StructVar* imExp) = 0;
    virtual void Visit(ImExp_Loc* imExp) = 0;
    virtual void Visit(ImExp_Exp* imExp) = 0;
};

template<class TFrom, class TVisitor>
concept ImExpConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept ImExpVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<ImExp_Namespace*>(), std::forward<TVisitorArgs>(args)...) } -> ImExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ImExp_GlobalFuncs*>(), std::forward<TVisitorArgs>(args)...) } -> ImExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ImExp_TypeVar*>(), std::forward<TVisitorArgs>(args)...) } -> ImExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ImExp_Class*>(), std::forward<TVisitorArgs>(args)...) } -> ImExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ImExp_ClassFuncs*>(), std::forward<TVisitorArgs>(args)...) } -> ImExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ImExp_Struct*>(), std::forward<TVisitorArgs>(args)...) } -> ImExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ImExp_StructFuncs*>(), std::forward<TVisitorArgs>(args)...) } -> ImExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ImExp_Enum*>(), std::forward<TVisitorArgs>(args)...) } -> ImExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ImExp_EnumElem*>(), std::forward<TVisitorArgs>(args)...) } -> ImExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ImExp_ClassVar*>(), std::forward<TVisitorArgs>(args)...) } -> ImExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ImExp_StructVar*>(), std::forward<TVisitorArgs>(args)...) } -> ImExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ImExp_Loc*>(), std::forward<TVisitorArgs>(args)...) } -> ImExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ImExp_Exp*>(), std::forward<TVisitorArgs>(args)...) } -> ImExpConvertibleToResultType<TVisitor>;

};

template<typename TVisitor, typename... TVisitorArgs> requires ImExpVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, ImExp* imExp, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : ImExpVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(ImExp_Namespace* imExp) override { call(imExp); }
            void Visit(ImExp_GlobalFuncs* imExp) override { call(imExp); }
            void Visit(ImExp_TypeVar* imExp) override { call(imExp); }
            void Visit(ImExp_Class* imExp) override { call(imExp); }
            void Visit(ImExp_ClassFuncs* imExp) override { call(imExp); }
            void Visit(ImExp_Struct* imExp) override { call(imExp); }
            void Visit(ImExp_StructFuncs* imExp) override { call(imExp); }
            void Visit(ImExp_Enum* imExp) override { call(imExp); }
            void Visit(ImExp_EnumElem* imExp) override { call(imExp); }
            void Visit(ImExp_ClassVar* imExp) override { call(imExp); }
            void Visit(ImExp_StructVar* imExp) override { call(imExp); }
            void Visit(ImExp_Loc* imExp) override { call(imExp); }
            void Visit(ImExp_Exp* imExp) override { call(imExp); }
        };

        Bridge bridge{caller};
        imExp->Accept(bridge);
    }
    else
    {
        struct Bridge : ImExpVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(ImExp_Namespace* imExp) override { result.emplace(call(imExp)); }
            void Visit(ImExp_GlobalFuncs* imExp) override { result.emplace(call(imExp)); }
            void Visit(ImExp_TypeVar* imExp) override { result.emplace(call(imExp)); }
            void Visit(ImExp_Class* imExp) override { result.emplace(call(imExp)); }
            void Visit(ImExp_ClassFuncs* imExp) override { result.emplace(call(imExp)); }
            void Visit(ImExp_Struct* imExp) override { result.emplace(call(imExp)); }
            void Visit(ImExp_StructFuncs* imExp) override { result.emplace(call(imExp)); }
            void Visit(ImExp_Enum* imExp) override { result.emplace(call(imExp)); }
            void Visit(ImExp_EnumElem* imExp) override { result.emplace(call(imExp)); }
            void Visit(ImExp_ClassVar* imExp) override { result.emplace(call(imExp)); }
            void Visit(ImExp_StructVar* imExp) override { result.emplace(call(imExp)); }
            void Visit(ImExp_Loc* imExp) override { result.emplace(call(imExp)); }
            void Visit(ImExp_Exp* imExp) override { result.emplace(call(imExp)); }
        };

        Bridge bridge{caller};
        imExp->Accept(bridge);
        return *bridge.result;
    }
}

} // namespace Citron