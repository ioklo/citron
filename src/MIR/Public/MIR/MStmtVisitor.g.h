#pragma once

#include <optional>

namespace Citron {
struct MStmtVisitor
{
    virtual ~MStmtVisitor() {}
    virtual void Visit(MStmt_Command* mStmt) = 0;
    virtual void Visit(MStmt_LocalVarDecl* mStmt) = 0;
    virtual void Visit(MStmt_LocalRefDecl* mStmt) = 0;
    virtual void Visit(MStmt_If* mStmt) = 0;
    virtual void Visit(MStmt_IfBind* mStmt) = 0;
    virtual void Visit(MStmt_For* mStmt) = 0;
    virtual void Visit(MStmt_Continue* mStmt) = 0;
    virtual void Visit(MStmt_Break* mStmt) = 0;
    virtual void Visit(MStmt_Return* mStmt) = 0;
    virtual void Visit(MStmt_Block* mStmt) = 0;
    virtual void Visit(MStmt_Blank* mStmt) = 0;
    virtual void Visit(MStmt_Exp* mStmt) = 0;
    virtual void Visit(MStmt_Task* mStmt) = 0;
    virtual void Visit(MStmt_Await* mStmt) = 0;
    virtual void Visit(MStmt_Async* mStmt) = 0;
    virtual void Visit(MStmt_Foreach* mStmt) = 0;
    virtual void Visit(MStmt_Yield* mStmt) = 0;
    virtual void Visit(MStmt_CallBaseClassCtor* mStmt) = 0;
    virtual void Visit(MStmt_CallBaseStructCtor* mStmt) = 0;
    virtual void Visit(MStmt_Directive* mStmt) = 0;
    virtual void Visit(MStmt_Call* mStmt) = 0;
    virtual void Visit(MStmt_Do* mStmt) = 0;
};

template<class TFrom, class TVisitor>
concept MStmtConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept MStmtVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<MStmt_Command*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_LocalVarDecl*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_LocalRefDecl*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_If*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_IfBind*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_For*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Continue*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Break*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Return*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Block*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Blank*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Exp*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Task*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Await*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Async*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Foreach*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Yield*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_CallBaseClassCtor*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_CallBaseStructCtor*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Directive*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Call*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Do*>(), std::forward<TVisitorArgs>(args)...) } -> MStmtConvertibleToResultType<TVisitor>;

};

template<typename TVisitor, typename... TVisitorArgs> requires MStmtVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, MStmt* mStmt, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : MStmtVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(MStmt_Command* mStmt) override { call(mStmt); }
            void Visit(MStmt_LocalVarDecl* mStmt) override { call(mStmt); }
            void Visit(MStmt_LocalRefDecl* mStmt) override { call(mStmt); }
            void Visit(MStmt_If* mStmt) override { call(mStmt); }
            void Visit(MStmt_IfBind* mStmt) override { call(mStmt); }
            void Visit(MStmt_For* mStmt) override { call(mStmt); }
            void Visit(MStmt_Continue* mStmt) override { call(mStmt); }
            void Visit(MStmt_Break* mStmt) override { call(mStmt); }
            void Visit(MStmt_Return* mStmt) override { call(mStmt); }
            void Visit(MStmt_Block* mStmt) override { call(mStmt); }
            void Visit(MStmt_Blank* mStmt) override { call(mStmt); }
            void Visit(MStmt_Exp* mStmt) override { call(mStmt); }
            void Visit(MStmt_Task* mStmt) override { call(mStmt); }
            void Visit(MStmt_Await* mStmt) override { call(mStmt); }
            void Visit(MStmt_Async* mStmt) override { call(mStmt); }
            void Visit(MStmt_Foreach* mStmt) override { call(mStmt); }
            void Visit(MStmt_Yield* mStmt) override { call(mStmt); }
            void Visit(MStmt_CallBaseClassCtor* mStmt) override { call(mStmt); }
            void Visit(MStmt_CallBaseStructCtor* mStmt) override { call(mStmt); }
            void Visit(MStmt_Directive* mStmt) override { call(mStmt); }
            void Visit(MStmt_Call* mStmt) override { call(mStmt); }
            void Visit(MStmt_Do* mStmt) override { call(mStmt); }
        };

        Bridge bridge{caller};
        mStmt->Accept(bridge);
    }
    else
    {
        struct Bridge : MStmtVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}            void Visit(MStmt_Command* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_LocalVarDecl* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_LocalRefDecl* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_If* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_IfBind* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_For* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_Continue* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_Break* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_Return* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_Block* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_Blank* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_Exp* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_Task* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_Await* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_Async* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_Foreach* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_Yield* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_CallBaseClassCtor* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_CallBaseStructCtor* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_Directive* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_Call* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_Do* mStmt) override { result.emplace(call(mStmt)); }
        };

        Bridge bridge{caller};
        mStmt->Accept(bridge);
        return *bridge.result;
    }
}

} // namespace Citron