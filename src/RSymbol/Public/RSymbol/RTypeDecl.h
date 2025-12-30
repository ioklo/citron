#pragma once


#include "RDecl.h"

namespace Citron {

class ETypeDecl;

class RClassDecl;
class RStructDecl;
class REnumDecl;
class REnumElemDecl;
class RInterfaceDecl;
class RLambdaDecl;

class RTypeDeclVisitor;

class RTypeDecl
{
public:
    virtual ~RTypeDecl() {}

    virtual RDecl* GetRDecl() = 0;
    virtual void Accept(RTypeDeclVisitor& visitor) = 0;
};

class RTypeDeclVisitor
{
public:
    virtual ~RTypeDeclVisitor() {}
    virtual void Visit(RClassDecl* typeDecl) = 0;
    virtual void Visit(RStructDecl* typeDecl) = 0;
    virtual void Visit(REnumDecl* typeDecl) = 0;
    virtual void Visit(REnumElemDecl* typeDecl) = 0;
    virtual void Visit(RInterfaceDecl* typeDecl) = 0;
    virtual void Visit(RLambdaDecl* typeDecl) = 0;
};

class RETypeDecl : public RTypeDecl
{
    ETypeDecl* typeDecl;
};

template<class TFrom, class TVisitor>
concept RTypeDeclConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept RTypeDeclVisitable = requires(TVisitor && v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;

    { v.Visit(std::declval<RClassDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RStructDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<REnumDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<REnumElemDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RInterfaceDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<RLambdaDecl*>(), std::forward<TVisitorArgs>(args)...) } -> RTypeDeclConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires RTypeDeclVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, RTypeDecl* typeDecl, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : RTypeDeclVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(RClassDecl* typeDecl) override { call(typeDecl); }
            void Visit(RStructDecl* typeDecl) override { call(typeDecl); }
            void Visit(REnumDecl* typeDecl) override { call(typeDecl); }
            void Visit(REnumElemDecl* typeDecl) override { call(typeDecl); }
            void Visit(RInterfaceDecl* typeDecl) override { call(typeDecl); }
            void Visit(RLambdaDecl* typeDecl) override { call(typeDecl); }
        };

        Bridge bridge{caller};
        typeDecl->Accept(bridge);
    }
    else
    {
        struct Bridge : RTypeDeclVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(RClassDecl* typeDecl) override { result.emplace(call(typeDecl)); }
            void Visit(RStructDecl* typeDecl) override { result.emplace(call(typeDecl)); }
            void Visit(REnumDecl* typeDecl) override { result.emplace(call(typeDecl)); }
            void Visit(REnumElemDecl* typeDecl) override { result.emplace(call(typeDecl)); }
            void Visit(RInterfaceDecl* typeDecl) override { result.emplace(call(typeDecl)); }
            void Visit(RLambdaDecl* typeDecl) override { result.emplace(call(typeDecl)); }
        };

        Bridge bridge{caller};
        typeDecl->Accept(bridge);
        return *bridge.result;
    }
}



} // namespace Citron