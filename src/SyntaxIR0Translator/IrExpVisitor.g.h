#pragma once

#include <optional>

namespace Citron {
struct IrExpVisitor
{
    virtual ~IrExpVisitor() {}
    virtual void Visit(IrExp_Namespace* irExp) = 0;
    virtual void Visit(IrExp_Class* irExp) = 0;
    virtual void Visit(IrExp_Struct* irExp) = 0;
    virtual void Visit(IrExp_Static* irExp) = 0;
    virtual void Visit(IrExp_ClassVar* irExp) = 0;
    virtual void Visit(IrExp_SharedStructVar* irExp) = 0;
    virtual void Visit(IrExp_StructVar* irExp) = 0;
    virtual void Visit(IrExp_Deref* irExp) = 0;
    virtual void Visit(IrExp_Exp* irExp) = 0;
    virtual void Visit(IrExp_Loc* irExp) = 0;
};

template<class TFrom, class TVisitor>
concept IrExpConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept IrExpVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<IrExp_Namespace*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_Class*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_Struct*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_Static*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_ClassVar*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_SharedStructVar*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_StructVar*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_Deref*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_Exp*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<IrExp_Loc*>(), std::forward<TVisitorArgs>(args)...) } -> IrExpConvertibleToResultType<TVisitor>;

};

template<typename TVisitor, typename... TVisitorArgs> requires IrExpVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, IrExp* irExp, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : IrExpVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(IrExp_Namespace* irExp) override { call(irExp); }
            void Visit(IrExp_Class* irExp) override { call(irExp); }
            void Visit(IrExp_Struct* irExp) override { call(irExp); }
            void Visit(IrExp_Static* irExp) override { call(irExp); }
            void Visit(IrExp_ClassVar* irExp) override { call(irExp); }
            void Visit(IrExp_SharedStructVar* irExp) override { call(irExp); }
            void Visit(IrExp_StructVar* irExp) override { call(irExp); }
            void Visit(IrExp_Deref* irExp) override { call(irExp); }
            void Visit(IrExp_Exp* irExp) override { call(irExp); }
            void Visit(IrExp_Loc* irExp) override { call(irExp); }
        };

        Bridge bridge{caller};
        irExp->Accept(bridge);
    }
    else
    {
        struct Bridge : IrExpVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(IrExp_Namespace* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_Class* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_Struct* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_Static* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_ClassVar* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_SharedStructVar* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_StructVar* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_Deref* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_Exp* irExp) override { result.emplace(call(irExp)); }
            void Visit(IrExp_Loc* irExp) override { result.emplace(call(irExp)); }
        };

        Bridge bridge{caller};
        irExp->Accept(bridge);
        return *bridge.result;
    }
}

} // namespace Citron