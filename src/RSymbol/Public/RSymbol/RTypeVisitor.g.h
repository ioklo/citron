#pragma once

#include <optional>

namespace Citron {
struct RTypeVisitor
{
    virtual ~RTypeVisitor() {}
    virtual void Visit(RType_Nullable* rType) = 0;
    virtual void Visit(RType_NullableInplace* rType) = 0;
    virtual void Visit(RType_TypeVar* rType) = 0;
    virtual void Visit(RType_Void* rType) = 0;
    virtual void Visit(RType_Primitive* rType) = 0;
    virtual void Visit(RType_Tuple* rType) = 0;
    virtual void Visit(RType_Func* rType) = 0;
    virtual void Visit(RType_Ptr* rType) = 0;
    virtual void Visit(RType_Shared* rType) = 0;
    virtual void Visit(RType_Box* rType) = 0;
    virtual void Visit(RType_Class* rType) = 0;
    virtual void Visit(RType_Struct* rType) = 0;
    virtual void Visit(RType_Enum* rType) = 0;
    virtual void Visit(RType_EnumElem* rType) = 0;
    virtual void Visit(RType_Interface* rType) = 0;
    virtual void Visit(RType_Lambda* rType) = 0;
};

template<class TFrom, class TVisitor>
concept RTypeConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept RTypeVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<RType_Nullable*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_NullableInplace*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_TypeVar*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Void*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Primitive*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Tuple*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Func*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Ptr*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Shared*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Box*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Class*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Struct*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Enum*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_EnumElem*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Interface*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RType_Lambda*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeConvertibleToResultType<TVisitor>;

};

template<typename TVisitor, typename... TVisitorArgs> requires RTypeVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, RType* rType, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : RTypeVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(RType_Nullable* rType) override { call(rType); }
            void Visit(RType_NullableInplace* rType) override { call(rType); }
            void Visit(RType_TypeVar* rType) override { call(rType); }
            void Visit(RType_Void* rType) override { call(rType); }
            void Visit(RType_Primitive* rType) override { call(rType); }
            void Visit(RType_Tuple* rType) override { call(rType); }
            void Visit(RType_Func* rType) override { call(rType); }
            void Visit(RType_Ptr* rType) override { call(rType); }
            void Visit(RType_Shared* rType) override { call(rType); }
            void Visit(RType_Box* rType) override { call(rType); }
            void Visit(RType_Class* rType) override { call(rType); }
            void Visit(RType_Struct* rType) override { call(rType); }
            void Visit(RType_Enum* rType) override { call(rType); }
            void Visit(RType_EnumElem* rType) override { call(rType); }
            void Visit(RType_Interface* rType) override { call(rType); }
            void Visit(RType_Lambda* rType) override { call(rType); }
        };

        Bridge bridge{caller};
        rType->Accept(bridge);
    }
    else
    {
        struct Bridge : RTypeVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(RType_Nullable* rType) override { result.emplace(call(rType)); }
            void Visit(RType_NullableInplace* rType) override { result.emplace(call(rType)); }
            void Visit(RType_TypeVar* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Void* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Primitive* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Tuple* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Func* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Ptr* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Shared* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Box* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Class* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Struct* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Enum* rType) override { result.emplace(call(rType)); }
            void Visit(RType_EnumElem* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Interface* rType) override { result.emplace(call(rType)); }
            void Visit(RType_Lambda* rType) override { result.emplace(call(rType)); }
        };

        Bridge bridge{caller};
        rType->Accept(bridge);
        return *bridge.result;
    }
}

} // namespace Citron