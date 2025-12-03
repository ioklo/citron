#pragma once

#include "MIRConfig.h"

#include <variant>
#include <vector>
#include <string>
#include <optional>

#include "RSymbol/RNames.h"
#include "MArgument.h"

namespace Citron {

class RType;
class RTypeArguments;

class RClassCtorDecl;

class MExp_String;
class NLambdaDecl;
class NStructCtorDecl;

class MLoc;

class MStmt_Command;
class MStmt_LocalVarDecl;
class MStmt_If;
class MStmt_IfNullableRefTest;
class MStmt_IfNullableValueTest;
class MStmt_For;
class MStmt_Continue;
class MStmt_Break;
class MStmt_Return;
class MStmt_Block;
class MStmt_Blank;
class MStmt_Exp;
class MStmt_Task;
class MStmt_Await;
class MStmt_Async;
class MStmt_Foreach;
class MStmt_ForeachCast;
class MStmt_Yield;
class MStmt_CallClassCtor;
class MStmt_CallStructCtor;
class MStmt_NullDirective;
class MStmt_NotNullDirective;
class MStmt_StaticNullDirective;
class MStmt_StaticNotNullDirective;
class MStmt_StaticUnknownNullDirective;

class MStmtVisitor
{
public:
    virtual ~MStmtVisitor() {}
    virtual void Visit(MStmt_Command* stmt) = 0;
    virtual void Visit(MStmt_LocalVarDecl* stmt) = 0;
    virtual void Visit(MStmt_If* stmt) = 0;
    virtual void Visit(MStmt_IfNullableRefTest* stmt) = 0;
    virtual void Visit(MStmt_IfNullableValueTest* stmt) = 0;
    virtual void Visit(MStmt_For* stmt) = 0;
    virtual void Visit(MStmt_Continue* stmt) = 0;
    virtual void Visit(MStmt_Break* stmt) = 0;
    virtual void Visit(MStmt_Return* stmt) = 0;
    virtual void Visit(MStmt_Block* stmt) = 0;
    virtual void Visit(MStmt_Blank* stmt) = 0;
    virtual void Visit(MStmt_Exp* stmt) = 0;
    virtual void Visit(MStmt_Task* stmt) = 0;
    virtual void Visit(MStmt_Await* stmt) = 0;
    virtual void Visit(MStmt_Async* stmt) = 0;
    virtual void Visit(MStmt_Foreach* stmt) = 0;
    virtual void Visit(MStmt_ForeachCast* stmt) = 0;
    virtual void Visit(MStmt_Yield* stmt) = 0;
    virtual void Visit(MStmt_CallClassCtor* stmt) = 0;
    virtual void Visit(MStmt_CallStructCtor* stmt) = 0;
    virtual void Visit(MStmt_NullDirective* stmt) = 0;
    virtual void Visit(MStmt_NotNullDirective* stmt) = 0;
    virtual void Visit(MStmt_StaticNullDirective* stmt) = 0;
    virtual void Visit(MStmt_StaticNotNullDirective* stmt) = 0;
    virtual void Visit(MStmt_StaticUnknownNullDirective* stmt) = 0;
};

class MStmt
{
public:
    virtual ~MStmt() {}
    virtual void Accept(MStmtVisitor& visitor) = 0;
};

class MStmt_Command : public MStmt
{
public:
    std::vector<MExp_String*> commands;
public:
    MIR_API MStmt_Command(std::vector<MExp_String*>&& commands);
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

// 로컬 변수는 
class MStmt_LocalVarDecl : public MStmt
{
public:
    RType* type;
    std::string name;
    MExp* initExp;
public:
    MIR_API MStmt_LocalVarDecl(RType* type, const std::string& name, MExp* initExp);
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_If : public MStmt
{
public:
    MExp* cond;
    std::vector<MStmt*> body;
    std::vector<MStmt*> elseBody;
public:
    MIR_API MStmt_If(MExp* cond, std::vector<MStmt*>&& body, std::vector<MStmt*>&& elseBody);
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_IfNullableRefTest : public MStmt
{
public:
    RType* refType;
    RName varName;
    MExp* asExp;
    std::vector<MStmt*> body;
    std::vector<MStmt*> elseBody;
public:
    MIR_API MStmt_IfNullableRefTest(RType* refType, RName&& varName, MExp* asExp, std::vector<MStmt*>&& body, std::vector<MStmt*>&& elseBody);

    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_IfNullableValueTest : public MStmt
{
public:
    RType* type;
    RName varName;
    MExp* asExp;
    std::vector<MStmt*> body;
    std::vector<MStmt*> elseBody;
public:
    MIR_API MStmt_IfNullableValueTest(RType* type, RName&& varName, MExp* asExp, std::vector<MStmt*>&& body, std::vector<MStmt*>&& elseBody);
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_For : public MStmt
{
public:
    std::vector<MStmt*> initStmts;
    MExp* condExp;
    MExp* continueExp;
    std::vector<MStmt*> body;
public:
    MIR_API MStmt_For(std::vector<MStmt*>&& initStmts, MExp* condExp, MExp* continueExp, std::vector<MStmt*>&& body);
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_Continue : public MStmt
{
public:
    MIR_API MStmt_Continue();
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_Break : public MStmt
{
public:
    MIR_API MStmt_Break();
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_Return : public MStmt
{
public:
    MExp* exp;
public:
    MIR_API MStmt_Return(MExp* exp);
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_Block : public MStmt
{
public:
    std::vector<MStmt*> stmts;
public:
    MIR_API MStmt_Block(std::vector<MStmt*>&& stmts);
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_Blank : public MStmt
{
public:
    MIR_API MStmt_Blank();
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_Exp : public MStmt
{
public:
    MExp* exp;
public:
    MIR_API MStmt_Exp(MExp* exp);
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_Task : public MStmt
{
public:
    NLambdaDecl* lambdaDecl;
    std::vector<MArgument> captureArgs;
public:
    MIR_API MStmt_Task(NLambdaDecl* lambdaDecl, std::vector<MArgument>&& captureArgs);
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_Await : public MStmt
{
public:
    std::vector<MStmt*> body;
public:
    MIR_API MStmt_Await(std::vector<MStmt*>&& body);
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_Async : public MStmt
{
public:
    NLambdaDecl* lambdaDecl;
    std::vector<MArgument> captureArgs;
public:
    MIR_API MStmt_Async(NLambdaDecl* lambdaDecl, std::vector<MArgument>&& captureArgs);
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_Foreach : public MStmt
{
public:
    MExp* enumeratorExp;
    RType* itemType;
    RName varName;
    MExp* nextExp;
    std::vector<MStmt*> body;
public:
    MIR_API MStmt_Foreach(MExp* enumeratorExp, RType* itemType, const RName& varName, MExp* nextExp, std::vector<MStmt*>&& body);
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_ForeachCast : public MStmt
{
public:
    MExp* enumeratorExp;
    RType* itemType;
    RName varName;
    RType* rawItemType;
    MExp* nextExp;
    MExp* castExp;
    std::vector<MStmt*> body;
public:
    MIR_API MStmt_ForeachCast(MExp* enumeratorExp, RType* itemType, const RName& varName, RType* rawItemType, MExp* nextExp, MExp* castExp, std::vector<MStmt*>&& body);
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_Yield : public MStmt
{
public:
    MExp* value;
public:
    MIR_API MStmt_Yield(MExp* value);
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

// Ctor 내에서 상위 Ctor 호출시 사용
class MStmt_CallClassCtor : public MStmt
{
public:
    RClassCtorDecl* ctor;
    std::vector<MArgument> args;
public:
    MIR_API MStmt_CallClassCtor();
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_CallStructCtor : public MStmt
{
public:
    NStructCtorDecl* ctor;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;
public:
    MIR_API MStmt_CallStructCtor();
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_NullDirective : public MStmt
{
public:
    MLoc* loc;
public:
    MIR_API MStmt_NullDirective(MLoc* loc);
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_NotNullDirective : public MStmt
{
public:
    MLoc* loc;
public:
    MIR_API MStmt_NotNullDirective(MLoc* loc);
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_StaticNullDirective : public MStmt
{
public:
    MLoc* loc;
public:
    MIR_API MStmt_StaticNullDirective();
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_StaticNotNullDirective : public MStmt
{
public:
    MLoc* loc;
public:
    MIR_API MStmt_StaticNotNullDirective();
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

class MStmt_StaticUnknownNullDirective : public MStmt
{
public:
    MLoc* loc;
public:
    MIR_API MStmt_StaticUnknownNullDirective();
    void Accept(MStmtVisitor& visitor) override { visitor.Visit(this); }
};

template<class TFrom, class TVisitor>
concept NStmtConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept NStmtVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;

    { v.Visit(std::declval<MStmt_Command*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_LocalVarDecl*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_If*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_IfNullableRefTest*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_IfNullableValueTest*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_For*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Continue*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Break*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Return*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Block*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Blank*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Exp*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Task*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Await*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Async*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Foreach*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_ForeachCast*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_Yield*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_CallClassCtor*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_CallStructCtor*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_NullDirective*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_NotNullDirective*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_StaticNullDirective*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_StaticNotNullDirective*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MStmt_StaticUnknownNullDirective*>(), std::forward<TVisitorArgs>(args)...) } -> NStmtConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires NStmtVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, MStmt* mStmt, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : MStmtVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(MStmt_Command* mStmt) override { call(mStmt); }
            void Visit(MStmt_LocalVarDecl* mStmt) override { call(mStmt); }
            void Visit(MStmt_If* mStmt) override { call(mStmt); }
            void Visit(MStmt_IfNullableRefTest* mStmt) override { call(mStmt); }
            void Visit(MStmt_IfNullableValueTest* mStmt) override { call(mStmt); }
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
            void Visit(MStmt_ForeachCast* mStmt) override { call(mStmt); }
            void Visit(MStmt_Yield* mStmt) override { call(mStmt); }
            void Visit(MStmt_CallClassCtor* mStmt) override { call(mStmt); }
            void Visit(MStmt_CallStructCtor* mStmt) override { call(mStmt); }
            void Visit(MStmt_NullDirective* mStmt) override { call(mStmt); }
            void Visit(MStmt_NotNullDirective* mStmt) override { call(mStmt); }
            void Visit(MStmt_StaticNullDirective* mStmt) override { call(mStmt); }
            void Visit(MStmt_StaticNotNullDirective* mStmt) override { call(mStmt); }
            void Visit(MStmt_StaticUnknownNullDirective* mStmt) override { call(mStmt); }
        };

        Bridge bridge{caller};
        mStmt->Accept(bridge);
    }
    else
    {
        struct Bridge : MStmtVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(MStmt_Command* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_LocalVarDecl* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_If* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_IfNullableRefTest* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_IfNullableValueTest* mStmt) override { result.emplace(call(mStmt)); }
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
            void Visit(MStmt_ForeachCast* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_Yield* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_CallClassCtor* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_CallStructCtor* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_NullDirective* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_NotNullDirective* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_StaticNullDirective* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_StaticNotNullDirective* mStmt) override { result.emplace(call(mStmt)); }
            void Visit(MStmt_StaticUnknownNullDirective* mStmt) override { result.emplace(call(mStmt)); }
        };

        Bridge bridge{caller};
        mStmt->Accept(bridge);
        return *bridge.result;
    }
}

}