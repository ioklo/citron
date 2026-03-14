#pragma once
#include "SyntaxConfig.h"

#include <string>
#include <vector>
#include <optional>
#include <memory>
#include <variant>

#include "Infra/Json.h"
#include "Infra/Unreachable.h"

namespace Citron {
class SFactory;
class SStmt;
class SStmt_Command;
class SStmt_VarDecl;
class SStmt_If;
class SStmt_IfBind;
class SStmt_For;
class SStmt_Continue;
class SStmt_Break;
class SStmt_Return;
class SStmt_Block;
class SStmt_Blank;
class SStmt_Exp;
class SStmt_Task;
class SStmt_Await;
class SStmt_Async;
class SStmt_Foreach;
class SStmt_Yield;
class SStmt_Directive;

class SExp;
class SExp_Identifier;
class SExp_String;
class SExp_IntLiteral;
class SExp_BoolLiteral;
class SExp_NullLiteral;
class SExp_BinaryOp;
class SExp_UnaryOp;
class SExp_Call;
class SExp_Lambda;
class SExp_Indexer;
class SExp_Member;
class SExp_List;
class SExp_New;
class SExp_Shared;
class SExp_Is;
class SExp_As;

class SVarDeclType;
class SVarDeclType_Var;
class SVarDeclType_VarRef;
class SVarDeclType_Ref;
class SVarDeclType_Normal;

class STypeExp;
class STypeExp_Id;
class STypeExp_Member;
class STypeExp_Nullable;
class STypeExp_Shared;
class STypeExp_Box;
class STypeExp_Ptr;
class STypeExp_Local;

class SStringExpElement;
class SStringExpElement_Text;
class SStringExpElement_Exp;

class SLambdaExpBody;
class SLambdaExpBody_Stmts;
class SLambdaExpBody_Exp;

class SEmbeddableStmt;
class SEmbeddableStmt_Single;
class SEmbeddableStmt_Block;

class SForStmtInitializer;
class SForStmtInitializer_Exp;
class SForStmtInitializer_VarDecl;

class SClassMemberDecl;
class SClassFuncDecl;
class SClassCtorDecl;
class SClassVarDecl;

class SStructMemberDecl;
class SStructFuncDecl;
class SStructCtorDecl;
class SStructDtorDecl;
class SStructVarDecl;

class SNamespaceDeclElement;
class SScriptElement;

class SClassDecl;
class SStructDecl;
class SEnumDecl;
class SGlobalFuncDecl;
class SNamespaceDecl;
class SScript;
class SArgument;
class SArguments;

enum class SAccessModifier
{
    Public,
    Protected,
    Private,
};

inline JsonItem ToJson(SAccessModifier& arg)
{
    switch(arg)
    {
    case SAccessModifier::Public: return JsonString("Public");
    case SAccessModifier::Protected: return JsonString("Protected");
    case SAccessModifier::Private: return JsonString("Private");
    }
    unreachable();
}

enum class SBinaryOpKind
{
    Multiply,
    Divide,
    Modulo,
    Add,
    Subtract,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,
    Equal,
    NotEqual,
    Assign,
};

inline JsonItem ToJson(SBinaryOpKind& arg)
{
    switch(arg)
    {
    case SBinaryOpKind::Multiply: return JsonString("Multiply");
    case SBinaryOpKind::Divide: return JsonString("Divide");
    case SBinaryOpKind::Modulo: return JsonString("Modulo");
    case SBinaryOpKind::Add: return JsonString("Add");
    case SBinaryOpKind::Subtract: return JsonString("Subtract");
    case SBinaryOpKind::LessThan: return JsonString("LessThan");
    case SBinaryOpKind::GreaterThan: return JsonString("GreaterThan");
    case SBinaryOpKind::LessThanOrEqual: return JsonString("LessThanOrEqual");
    case SBinaryOpKind::GreaterThanOrEqual: return JsonString("GreaterThanOrEqual");
    case SBinaryOpKind::Equal: return JsonString("Equal");
    case SBinaryOpKind::NotEqual: return JsonString("NotEqual");
    case SBinaryOpKind::Assign: return JsonString("Assign");
    }
    unreachable();
}

enum class SUnaryOpKind
{
    PostfixInc,
    PostfixDec,
    Minus,
    LogicalNot,
    PrefixInc,
    PrefixDec,
    Ref,
    Deref,
};

inline JsonItem ToJson(SUnaryOpKind& arg)
{
    switch(arg)
    {
    case SUnaryOpKind::PostfixInc: return JsonString("PostfixInc");
    case SUnaryOpKind::PostfixDec: return JsonString("PostfixDec");
    case SUnaryOpKind::Minus: return JsonString("Minus");
    case SUnaryOpKind::LogicalNot: return JsonString("LogicalNot");
    case SUnaryOpKind::PrefixInc: return JsonString("PrefixInc");
    case SUnaryOpKind::PrefixDec: return JsonString("PrefixDec");
    case SUnaryOpKind::Ref: return JsonString("Ref");
    case SUnaryOpKind::Deref: return JsonString("Deref");
    }
    unreachable();
}

enum class SParamModifier
{
    In,
    Move,
    Forward,
    Out,
    Params,
};

inline JsonItem ToJson(SParamModifier& arg)
{
    switch(arg)
    {
    case SParamModifier::In: return JsonString("In");
    case SParamModifier::Move: return JsonString("Move");
    case SParamModifier::Forward: return JsonString("Forward");
    case SParamModifier::Out: return JsonString("Out");
    case SParamModifier::Params: return JsonString("Params");
    }
    unreachable();
}

enum class SArgModifier
{
    Ref,
    Move,
    Forward,
    Out,
    Params,
};

inline JsonItem ToJson(SArgModifier& arg)
{
    switch(arg)
    {
    case SArgModifier::Ref: return JsonString("Ref");
    case SArgModifier::Move: return JsonString("Move");
    case SArgModifier::Forward: return JsonString("Forward");
    case SArgModifier::Out: return JsonString("Out");
    case SArgModifier::Params: return JsonString("Params");
    }
    unreachable();
}

class SSyntax
{
public:
    SYNTAX_API SSyntax();
    SSyntax(const SSyntax&) = delete;
    SYNTAX_API SSyntax(SSyntax&&) noexcept;
    SYNTAX_API virtual ~SSyntax();

    SSyntax& operator=(const SSyntax& other) = delete;
    SYNTAX_API SSyntax& operator=(SSyntax&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class SArgument
{
public:
    std::optional<SArgModifier> o_modifier;
    SExp* exp;

    SYNTAX_API SArgument(std::optional<SArgModifier> o_modifier, SExp* exp);
    SYNTAX_API SArgument(SExp* exp);
    SArgument(const SArgument&) = delete;
    SYNTAX_API SArgument(SArgument&&) noexcept;
    SYNTAX_API ~SArgument();

    SArgument& operator=(const SArgument& other) = delete;
    SYNTAX_API SArgument& operator=(SArgument&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class SArguments
{
public:
    std::vector<SArgument*> items;

    SYNTAX_API SArguments(std::vector<SArgument*> items);
    SArguments(const SArguments&) = delete;
    SYNTAX_API SArguments(SArguments&&) noexcept;
    SYNTAX_API ~SArguments();

    SArguments& operator=(const SArguments& other) = delete;
    SYNTAX_API SArguments& operator=(SArguments&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class SLambdaExpParam
{
public:
    std::optional<SParamModifier> o_paramModifier;
    STypeExp* type;
    std::string memberName;

    SYNTAX_API SLambdaExpParam(std::optional<SParamModifier> o_paramModifier, STypeExp* type, std::string memberName);
    SLambdaExpParam(const SLambdaExpParam&) = delete;
    SYNTAX_API SLambdaExpParam(SLambdaExpParam&&) noexcept;
    SYNTAX_API ~SLambdaExpParam();

    SLambdaExpParam& operator=(const SLambdaExpParam& other) = delete;
    SYNTAX_API SLambdaExpParam& operator=(SLambdaExpParam&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

struct SVarDeclElementInit_Uninit
{
    SVarDeclElementInit_Uninit() { }

    SYNTAX_API JsonItem ToJson();
};

struct SVarDeclElementInit_Exp
{
    SExp* exp;

    SVarDeclElementInit_Exp(SExp* exp)
        : exp{exp} { }

    SYNTAX_API JsonItem ToJson();
};

struct SVarDeclElementInit_Move
{
    SExp* exp;

    SVarDeclElementInit_Move(SExp* exp)
        : exp{exp} { }

    SYNTAX_API JsonItem ToJson();
};

using SVarDeclElementInit = std::variant<
    SVarDeclElementInit_Uninit,
    SVarDeclElementInit_Exp,
    SVarDeclElementInit_Move>;

SYNTAX_API JsonItem ToJson(SVarDeclElementInit& init);

class SVarDeclElement
{
public:
    std::string varName;
    SVarDeclElementInit init;

    SYNTAX_API SVarDeclElement(std::string varName, SVarDeclElementInit init);
    SVarDeclElement(const SVarDeclElement&) = delete;
    SYNTAX_API SVarDeclElement(SVarDeclElement&&) noexcept;
    SYNTAX_API ~SVarDeclElement();

    SVarDeclElement& operator=(const SVarDeclElement& other) = delete;
    SYNTAX_API SVarDeclElement& operator=(SVarDeclElement&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class SVarDecl
{
public:
    SVarDeclType* type;
    std::vector<SVarDeclElement> elements;

    SYNTAX_API SVarDecl(SVarDeclType* type, std::vector<SVarDeclElement> elements);
    SVarDecl(const SVarDecl&) = delete;
    SYNTAX_API SVarDecl(SVarDecl&&) noexcept;
    SYNTAX_API ~SVarDecl();

    SVarDecl& operator=(const SVarDecl& other) = delete;
    SYNTAX_API SVarDecl& operator=(SVarDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class STypeParam
{
public:
    std::string memberName;

    SYNTAX_API STypeParam(std::string memberName);
    STypeParam(const STypeParam&) = delete;
    SYNTAX_API STypeParam(STypeParam&&) noexcept;
    SYNTAX_API ~STypeParam();

    STypeParam& operator=(const STypeParam& other) = delete;
    SYNTAX_API STypeParam& operator=(STypeParam&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class SFuncParam
{
public:
    std::optional<SParamModifier> o_modifier;
    bool bRef;
    STypeExp* type;
    std::string memberName;

    SYNTAX_API SFuncParam(std::optional<SParamModifier> o_modifier, bool bRef, STypeExp* type, std::string memberName);
    SFuncParam(const SFuncParam&) = delete;
    SYNTAX_API SFuncParam(SFuncParam&&) noexcept;
    SYNTAX_API ~SFuncParam();

    SFuncParam& operator=(const SFuncParam& other) = delete;
    SYNTAX_API SFuncParam& operator=(SFuncParam&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class SStmtVisitor
{
public:
    virtual ~SStmtVisitor() = default;
    virtual void Visit(SStmt_Command* stmt) = 0;
    virtual void Visit(SStmt_VarDecl* stmt) = 0;
    virtual void Visit(SStmt_If* stmt) = 0;
    virtual void Visit(SStmt_IfBind* stmt) = 0;
    virtual void Visit(SStmt_For* stmt) = 0;
    virtual void Visit(SStmt_Continue* stmt) = 0;
    virtual void Visit(SStmt_Break* stmt) = 0;
    virtual void Visit(SStmt_Return* stmt) = 0;
    virtual void Visit(SStmt_Block* stmt) = 0;
    virtual void Visit(SStmt_Blank* stmt) = 0;
    virtual void Visit(SStmt_Exp* stmt) = 0;
    virtual void Visit(SStmt_Task* stmt) = 0;
    virtual void Visit(SStmt_Await* stmt) = 0;
    virtual void Visit(SStmt_Async* stmt) = 0;
    virtual void Visit(SStmt_Foreach* stmt) = 0;
    virtual void Visit(SStmt_Yield* stmt) = 0;
    virtual void Visit(SStmt_Directive* stmt) = 0;
};

class SStmt : virtual public SSyntax
{
public:
    SStmt() = default;
    SStmt(const SStmt&) = delete;
    SStmt(SStmt&&) = default;
    virtual ~SStmt() { }
    SStmt& operator=(const SStmt& other) = delete;
    SStmt& operator=(SStmt&& other) noexcept = default;
    virtual void Accept(SStmtVisitor& visitor) = 0;
};

template<class TFrom, class TVisitor>
concept SStmtConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;
// ResultType은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept SStmtVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<SStmt_Command*>(), std::forward<TVisitorArgs>(args)...) } -> SStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStmt_VarDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStmt_If*>(), std::forward<TVisitorArgs>(args)...) } -> SStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStmt_IfBind*>(), std::forward<TVisitorArgs>(args)...) } -> SStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStmt_For*>(), std::forward<TVisitorArgs>(args)...) } -> SStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStmt_Continue*>(), std::forward<TVisitorArgs>(args)...) } -> SStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStmt_Break*>(), std::forward<TVisitorArgs>(args)...) } -> SStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStmt_Return*>(), std::forward<TVisitorArgs>(args)...) } -> SStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStmt_Block*>(), std::forward<TVisitorArgs>(args)...) } -> SStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStmt_Blank*>(), std::forward<TVisitorArgs>(args)...) } -> SStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStmt_Exp*>(), std::forward<TVisitorArgs>(args)...) } -> SStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStmt_Task*>(), std::forward<TVisitorArgs>(args)...) } -> SStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStmt_Await*>(), std::forward<TVisitorArgs>(args)...) } -> SStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStmt_Async*>(), std::forward<TVisitorArgs>(args)...) } -> SStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStmt_Foreach*>(), std::forward<TVisitorArgs>(args)...) } -> SStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStmt_Yield*>(), std::forward<TVisitorArgs>(args)...) } -> SStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStmt_Directive*>(), std::forward<TVisitorArgs>(args)...) } -> SStmtConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires SStmtVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, SStmt* stmt, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : SStmtVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(SStmt_Command* stmt) override { call(stmt); }
            void Visit(SStmt_VarDecl* stmt) override { call(stmt); }
            void Visit(SStmt_If* stmt) override { call(stmt); }
            void Visit(SStmt_IfBind* stmt) override { call(stmt); }
            void Visit(SStmt_For* stmt) override { call(stmt); }
            void Visit(SStmt_Continue* stmt) override { call(stmt); }
            void Visit(SStmt_Break* stmt) override { call(stmt); }
            void Visit(SStmt_Return* stmt) override { call(stmt); }
            void Visit(SStmt_Block* stmt) override { call(stmt); }
            void Visit(SStmt_Blank* stmt) override { call(stmt); }
            void Visit(SStmt_Exp* stmt) override { call(stmt); }
            void Visit(SStmt_Task* stmt) override { call(stmt); }
            void Visit(SStmt_Await* stmt) override { call(stmt); }
            void Visit(SStmt_Async* stmt) override { call(stmt); }
            void Visit(SStmt_Foreach* stmt) override { call(stmt); }
            void Visit(SStmt_Yield* stmt) override { call(stmt); }
            void Visit(SStmt_Directive* stmt) override { call(stmt); }
        };

        Bridge bridge{caller};
        stmt->Accept(bridge);
    }
    else
    {
        struct Bridge : SStmtVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(SStmt_Command* stmt) override { result.emplace(call(stmt)); }
            void Visit(SStmt_VarDecl* stmt) override { result.emplace(call(stmt)); }
            void Visit(SStmt_If* stmt) override { result.emplace(call(stmt)); }
            void Visit(SStmt_IfBind* stmt) override { result.emplace(call(stmt)); }
            void Visit(SStmt_For* stmt) override { result.emplace(call(stmt)); }
            void Visit(SStmt_Continue* stmt) override { result.emplace(call(stmt)); }
            void Visit(SStmt_Break* stmt) override { result.emplace(call(stmt)); }
            void Visit(SStmt_Return* stmt) override { result.emplace(call(stmt)); }
            void Visit(SStmt_Block* stmt) override { result.emplace(call(stmt)); }
            void Visit(SStmt_Blank* stmt) override { result.emplace(call(stmt)); }
            void Visit(SStmt_Exp* stmt) override { result.emplace(call(stmt)); }
            void Visit(SStmt_Task* stmt) override { result.emplace(call(stmt)); }
            void Visit(SStmt_Await* stmt) override { result.emplace(call(stmt)); }
            void Visit(SStmt_Async* stmt) override { result.emplace(call(stmt)); }
            void Visit(SStmt_Foreach* stmt) override { result.emplace(call(stmt)); }
            void Visit(SStmt_Yield* stmt) override { result.emplace(call(stmt)); }
            void Visit(SStmt_Directive* stmt) override { result.emplace(call(stmt)); }
        };

        Bridge bridge{caller};
        stmt->Accept(bridge);
        return *bridge.result;
    }
}

SYNTAX_API JsonItem ToJson(SStmt* stmt);

class SExpVisitor
{
public:
    virtual ~SExpVisitor() = default;
    virtual void Visit(SExp_Identifier* exp) = 0;
    virtual void Visit(SExp_String* exp) = 0;
    virtual void Visit(SExp_IntLiteral* exp) = 0;
    virtual void Visit(SExp_BoolLiteral* exp) = 0;
    virtual void Visit(SExp_NullLiteral* exp) = 0;
    virtual void Visit(SExp_BinaryOp* exp) = 0;
    virtual void Visit(SExp_UnaryOp* exp) = 0;
    virtual void Visit(SExp_Call* exp) = 0;
    virtual void Visit(SExp_Lambda* exp) = 0;
    virtual void Visit(SExp_Indexer* exp) = 0;
    virtual void Visit(SExp_Member* exp) = 0;
    virtual void Visit(SExp_List* exp) = 0;
    virtual void Visit(SExp_New* exp) = 0;
    virtual void Visit(SExp_Shared* exp) = 0;
    virtual void Visit(SExp_Is* exp) = 0;
    virtual void Visit(SExp_As* exp) = 0;
};

class SExp : virtual public SSyntax
{
public:
    SExp() = default;
    SExp(const SExp&) = delete;
    SExp(SExp&&) = default;
    virtual ~SExp() { }
    SExp& operator=(const SExp& other) = delete;
    SExp& operator=(SExp&& other) noexcept = default;
    virtual void Accept(SExpVisitor& visitor) = 0;
};

template<class TFrom, class TVisitor>
concept SExpConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;
// ResultType은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept SExpVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<SExp_Identifier*>(), std::forward<TVisitorArgs>(args)...) } -> SExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SExp_String*>(), std::forward<TVisitorArgs>(args)...) } -> SExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SExp_IntLiteral*>(), std::forward<TVisitorArgs>(args)...) } -> SExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SExp_BoolLiteral*>(), std::forward<TVisitorArgs>(args)...) } -> SExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SExp_NullLiteral*>(), std::forward<TVisitorArgs>(args)...) } -> SExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SExp_BinaryOp*>(), std::forward<TVisitorArgs>(args)...) } -> SExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SExp_UnaryOp*>(), std::forward<TVisitorArgs>(args)...) } -> SExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SExp_Call*>(), std::forward<TVisitorArgs>(args)...) } -> SExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SExp_Lambda*>(), std::forward<TVisitorArgs>(args)...) } -> SExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SExp_Indexer*>(), std::forward<TVisitorArgs>(args)...) } -> SExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SExp_Member*>(), std::forward<TVisitorArgs>(args)...) } -> SExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SExp_List*>(), std::forward<TVisitorArgs>(args)...) } -> SExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SExp_New*>(), std::forward<TVisitorArgs>(args)...) } -> SExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SExp_Shared*>(), std::forward<TVisitorArgs>(args)...) } -> SExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SExp_Is*>(), std::forward<TVisitorArgs>(args)...) } -> SExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SExp_As*>(), std::forward<TVisitorArgs>(args)...) } -> SExpConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires SExpVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, SExp* exp, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : SExpVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(SExp_Identifier* exp) override { call(exp); }
            void Visit(SExp_String* exp) override { call(exp); }
            void Visit(SExp_IntLiteral* exp) override { call(exp); }
            void Visit(SExp_BoolLiteral* exp) override { call(exp); }
            void Visit(SExp_NullLiteral* exp) override { call(exp); }
            void Visit(SExp_BinaryOp* exp) override { call(exp); }
            void Visit(SExp_UnaryOp* exp) override { call(exp); }
            void Visit(SExp_Call* exp) override { call(exp); }
            void Visit(SExp_Lambda* exp) override { call(exp); }
            void Visit(SExp_Indexer* exp) override { call(exp); }
            void Visit(SExp_Member* exp) override { call(exp); }
            void Visit(SExp_List* exp) override { call(exp); }
            void Visit(SExp_New* exp) override { call(exp); }
            void Visit(SExp_Shared* exp) override { call(exp); }
            void Visit(SExp_Is* exp) override { call(exp); }
            void Visit(SExp_As* exp) override { call(exp); }
        };

        Bridge bridge{caller};
        exp->Accept(bridge);
    }
    else
    {
        struct Bridge : SExpVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(SExp_Identifier* exp) override { result.emplace(call(exp)); }
            void Visit(SExp_String* exp) override { result.emplace(call(exp)); }
            void Visit(SExp_IntLiteral* exp) override { result.emplace(call(exp)); }
            void Visit(SExp_BoolLiteral* exp) override { result.emplace(call(exp)); }
            void Visit(SExp_NullLiteral* exp) override { result.emplace(call(exp)); }
            void Visit(SExp_BinaryOp* exp) override { result.emplace(call(exp)); }
            void Visit(SExp_UnaryOp* exp) override { result.emplace(call(exp)); }
            void Visit(SExp_Call* exp) override { result.emplace(call(exp)); }
            void Visit(SExp_Lambda* exp) override { result.emplace(call(exp)); }
            void Visit(SExp_Indexer* exp) override { result.emplace(call(exp)); }
            void Visit(SExp_Member* exp) override { result.emplace(call(exp)); }
            void Visit(SExp_List* exp) override { result.emplace(call(exp)); }
            void Visit(SExp_New* exp) override { result.emplace(call(exp)); }
            void Visit(SExp_Shared* exp) override { result.emplace(call(exp)); }
            void Visit(SExp_Is* exp) override { result.emplace(call(exp)); }
            void Visit(SExp_As* exp) override { result.emplace(call(exp)); }
        };

        Bridge bridge{caller};
        exp->Accept(bridge);
        return *bridge.result;
    }
}

SYNTAX_API JsonItem ToJson(SExp* exp);

class STypeExpVisitor
{
public:
    virtual ~STypeExpVisitor() = default;
    virtual void Visit(STypeExp_Id* typeExp) = 0;
    virtual void Visit(STypeExp_Member* typeExp) = 0;
    virtual void Visit(STypeExp_Nullable* typeExp) = 0;
    virtual void Visit(STypeExp_Shared* typeExp) = 0;
    virtual void Visit(STypeExp_Box* typeExp) = 0;
    virtual void Visit(STypeExp_Ptr* typeExp) = 0;
    virtual void Visit(STypeExp_Local* typeExp) = 0;
};

class STypeExp : virtual public SSyntax
{
public:
    STypeExp() = default;
    STypeExp(const STypeExp&) = delete;
    STypeExp(STypeExp&&) = default;
    virtual ~STypeExp() { }
    STypeExp& operator=(const STypeExp& other) = delete;
    STypeExp& operator=(STypeExp&& other) noexcept = default;
    virtual void Accept(STypeExpVisitor& visitor) = 0;
};

template<class TFrom, class TVisitor>
concept STypeExpConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;
// ResultType은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept STypeExpVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<STypeExp_Id*>(), std::forward<TVisitorArgs>(args)...) } -> STypeExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<STypeExp_Member*>(), std::forward<TVisitorArgs>(args)...) } -> STypeExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<STypeExp_Nullable*>(), std::forward<TVisitorArgs>(args)...) } -> STypeExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<STypeExp_Shared*>(), std::forward<TVisitorArgs>(args)...) } -> STypeExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<STypeExp_Box*>(), std::forward<TVisitorArgs>(args)...) } -> STypeExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<STypeExp_Ptr*>(), std::forward<TVisitorArgs>(args)...) } -> STypeExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<STypeExp_Local*>(), std::forward<TVisitorArgs>(args)...) } -> STypeExpConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires STypeExpVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, STypeExp* typeExp, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : STypeExpVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(STypeExp_Id* typeExp) override { call(typeExp); }
            void Visit(STypeExp_Member* typeExp) override { call(typeExp); }
            void Visit(STypeExp_Nullable* typeExp) override { call(typeExp); }
            void Visit(STypeExp_Shared* typeExp) override { call(typeExp); }
            void Visit(STypeExp_Box* typeExp) override { call(typeExp); }
            void Visit(STypeExp_Ptr* typeExp) override { call(typeExp); }
            void Visit(STypeExp_Local* typeExp) override { call(typeExp); }
        };

        Bridge bridge{caller};
        typeExp->Accept(bridge);
    }
    else
    {
        struct Bridge : STypeExpVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(STypeExp_Id* typeExp) override { result.emplace(call(typeExp)); }
            void Visit(STypeExp_Member* typeExp) override { result.emplace(call(typeExp)); }
            void Visit(STypeExp_Nullable* typeExp) override { result.emplace(call(typeExp)); }
            void Visit(STypeExp_Shared* typeExp) override { result.emplace(call(typeExp)); }
            void Visit(STypeExp_Box* typeExp) override { result.emplace(call(typeExp)); }
            void Visit(STypeExp_Ptr* typeExp) override { result.emplace(call(typeExp)); }
            void Visit(STypeExp_Local* typeExp) override { result.emplace(call(typeExp)); }
        };

        Bridge bridge{caller};
        typeExp->Accept(bridge);
        return *bridge.result;
    }
}

SYNTAX_API JsonItem ToJson(STypeExp* typeExp);

class SStringExpElementVisitor
{
public:
    virtual ~SStringExpElementVisitor() = default;
    virtual void Visit(SStringExpElement_Text* elem) = 0;
    virtual void Visit(SStringExpElement_Exp* elem) = 0;
};

class SStringExpElement : virtual public SSyntax
{
public:
    SStringExpElement() = default;
    SStringExpElement(const SStringExpElement&) = delete;
    SStringExpElement(SStringExpElement&&) = default;
    virtual ~SStringExpElement() { }
    SStringExpElement& operator=(const SStringExpElement& other) = delete;
    SStringExpElement& operator=(SStringExpElement&& other) noexcept = default;
    virtual void Accept(SStringExpElementVisitor& visitor) = 0;
};

template<class TFrom, class TVisitor>
concept SStringExpElementConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;
// ResultType은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept SStringExpElementVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<SStringExpElement_Text*>(), std::forward<TVisitorArgs>(args)...) } -> SStringExpElementConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStringExpElement_Exp*>(), std::forward<TVisitorArgs>(args)...) } -> SStringExpElementConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires SStringExpElementVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, SStringExpElement* elem, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : SStringExpElementVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(SStringExpElement_Text* elem) override { call(elem); }
            void Visit(SStringExpElement_Exp* elem) override { call(elem); }
        };

        Bridge bridge{caller};
        elem->Accept(bridge);
    }
    else
    {
        struct Bridge : SStringExpElementVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(SStringExpElement_Text* elem) override { result.emplace(call(elem)); }
            void Visit(SStringExpElement_Exp* elem) override { result.emplace(call(elem)); }
        };

        Bridge bridge{caller};
        elem->Accept(bridge);
        return *bridge.result;
    }
}

SYNTAX_API JsonItem ToJson(SStringExpElement* elem);

class SLambdaExpBodyVisitor
{
public:
    virtual ~SLambdaExpBodyVisitor() = default;
    virtual void Visit(SLambdaExpBody_Stmts* body) = 0;
    virtual void Visit(SLambdaExpBody_Exp* body) = 0;
};

class SLambdaExpBody : virtual public SSyntax
{
public:
    SLambdaExpBody() = default;
    SLambdaExpBody(const SLambdaExpBody&) = delete;
    SLambdaExpBody(SLambdaExpBody&&) = default;
    virtual ~SLambdaExpBody() { }
    SLambdaExpBody& operator=(const SLambdaExpBody& other) = delete;
    SLambdaExpBody& operator=(SLambdaExpBody&& other) noexcept = default;
    virtual void Accept(SLambdaExpBodyVisitor& visitor) = 0;
};

template<class TFrom, class TVisitor>
concept SLambdaExpBodyConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;
// ResultType은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept SLambdaExpBodyVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<SLambdaExpBody_Stmts*>(), std::forward<TVisitorArgs>(args)...) } -> SLambdaExpBodyConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SLambdaExpBody_Exp*>(), std::forward<TVisitorArgs>(args)...) } -> SLambdaExpBodyConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires SLambdaExpBodyVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, SLambdaExpBody* body, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : SLambdaExpBodyVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(SLambdaExpBody_Stmts* body) override { call(body); }
            void Visit(SLambdaExpBody_Exp* body) override { call(body); }
        };

        Bridge bridge{caller};
        body->Accept(bridge);
    }
    else
    {
        struct Bridge : SLambdaExpBodyVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(SLambdaExpBody_Stmts* body) override { result.emplace(call(body)); }
            void Visit(SLambdaExpBody_Exp* body) override { result.emplace(call(body)); }
        };

        Bridge bridge{caller};
        body->Accept(bridge);
        return *bridge.result;
    }
}

SYNTAX_API JsonItem ToJson(SLambdaExpBody* body);

class SEmbeddableStmtVisitor
{
public:
    virtual ~SEmbeddableStmtVisitor() = default;
    virtual void Visit(SEmbeddableStmt_Single* stmt) = 0;
    virtual void Visit(SEmbeddableStmt_Block* stmt) = 0;
};

class SEmbeddableStmt : virtual public SSyntax
{
public:
    SEmbeddableStmt() = default;
    SEmbeddableStmt(const SEmbeddableStmt&) = delete;
    SEmbeddableStmt(SEmbeddableStmt&&) = default;
    virtual ~SEmbeddableStmt() { }
    SEmbeddableStmt& operator=(const SEmbeddableStmt& other) = delete;
    SEmbeddableStmt& operator=(SEmbeddableStmt&& other) noexcept = default;
    virtual void Accept(SEmbeddableStmtVisitor& visitor) = 0;
};

template<class TFrom, class TVisitor>
concept SEmbeddableStmtConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;
// ResultType은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept SEmbeddableStmtVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<SEmbeddableStmt_Single*>(), std::forward<TVisitorArgs>(args)...) } -> SEmbeddableStmtConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SEmbeddableStmt_Block*>(), std::forward<TVisitorArgs>(args)...) } -> SEmbeddableStmtConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires SEmbeddableStmtVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, SEmbeddableStmt* stmt, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : SEmbeddableStmtVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(SEmbeddableStmt_Single* stmt) override { call(stmt); }
            void Visit(SEmbeddableStmt_Block* stmt) override { call(stmt); }
        };

        Bridge bridge{caller};
        stmt->Accept(bridge);
    }
    else
    {
        struct Bridge : SEmbeddableStmtVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(SEmbeddableStmt_Single* stmt) override { result.emplace(call(stmt)); }
            void Visit(SEmbeddableStmt_Block* stmt) override { result.emplace(call(stmt)); }
        };

        Bridge bridge{caller};
        stmt->Accept(bridge);
        return *bridge.result;
    }
}

SYNTAX_API JsonItem ToJson(SEmbeddableStmt* stmt);

class SForStmtInitializerVisitor
{
public:
    virtual ~SForStmtInitializerVisitor() = default;
    virtual void Visit(SForStmtInitializer_Exp* initializer) = 0;
    virtual void Visit(SForStmtInitializer_VarDecl* initializer) = 0;
};

class SForStmtInitializer : virtual public SSyntax
{
public:
    SForStmtInitializer() = default;
    SForStmtInitializer(const SForStmtInitializer&) = delete;
    SForStmtInitializer(SForStmtInitializer&&) = default;
    virtual ~SForStmtInitializer() { }
    SForStmtInitializer& operator=(const SForStmtInitializer& other) = delete;
    SForStmtInitializer& operator=(SForStmtInitializer&& other) noexcept = default;
    virtual void Accept(SForStmtInitializerVisitor& visitor) = 0;
};

template<class TFrom, class TVisitor>
concept SForStmtInitializerConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;
// ResultType은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept SForStmtInitializerVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<SForStmtInitializer_Exp*>(), std::forward<TVisitorArgs>(args)...) } -> SForStmtInitializerConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SForStmtInitializer_VarDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SForStmtInitializerConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires SForStmtInitializerVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, SForStmtInitializer* initializer, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : SForStmtInitializerVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(SForStmtInitializer_Exp* initializer) override { call(initializer); }
            void Visit(SForStmtInitializer_VarDecl* initializer) override { call(initializer); }
        };

        Bridge bridge{caller};
        initializer->Accept(bridge);
    }
    else
    {
        struct Bridge : SForStmtInitializerVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(SForStmtInitializer_Exp* initializer) override { result.emplace(call(initializer)); }
            void Visit(SForStmtInitializer_VarDecl* initializer) override { result.emplace(call(initializer)); }
        };

        Bridge bridge{caller};
        initializer->Accept(bridge);
        return *bridge.result;
    }
}

SYNTAX_API JsonItem ToJson(SForStmtInitializer* initializer);

class SClassMemberDeclVisitor
{
public:
    virtual ~SClassMemberDeclVisitor() = default;
    virtual void Visit(SClassDecl* decl) = 0;
    virtual void Visit(SStructDecl* decl) = 0;
    virtual void Visit(SEnumDecl* decl) = 0;
    virtual void Visit(SClassFuncDecl* decl) = 0;
    virtual void Visit(SClassCtorDecl* decl) = 0;
    virtual void Visit(SClassVarDecl* decl) = 0;
};

class SClassMemberDecl : virtual public SSyntax
{
public:
    SClassMemberDecl() = default;
    SClassMemberDecl(const SClassMemberDecl&) = delete;
    SClassMemberDecl(SClassMemberDecl&&) = default;
    virtual ~SClassMemberDecl() { }
    SClassMemberDecl& operator=(const SClassMemberDecl& other) = delete;
    SClassMemberDecl& operator=(SClassMemberDecl&& other) noexcept = default;
    virtual void Accept(SClassMemberDeclVisitor& visitor) = 0;
};

template<class TFrom, class TVisitor>
concept SClassMemberDeclConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;
// ResultType은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept SClassMemberDeclVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<SClassDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SClassMemberDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStructDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SClassMemberDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SEnumDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SClassMemberDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SClassFuncDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SClassMemberDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SClassCtorDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SClassMemberDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SClassVarDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SClassMemberDeclConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires SClassMemberDeclVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, SClassMemberDecl* decl, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : SClassMemberDeclVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(SClassDecl* decl) override { call(decl); }
            void Visit(SStructDecl* decl) override { call(decl); }
            void Visit(SEnumDecl* decl) override { call(decl); }
            void Visit(SClassFuncDecl* decl) override { call(decl); }
            void Visit(SClassCtorDecl* decl) override { call(decl); }
            void Visit(SClassVarDecl* decl) override { call(decl); }
        };

        Bridge bridge{caller};
        decl->Accept(bridge);
    }
    else
    {
        struct Bridge : SClassMemberDeclVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(SClassDecl* decl) override { result.emplace(call(decl)); }
            void Visit(SStructDecl* decl) override { result.emplace(call(decl)); }
            void Visit(SEnumDecl* decl) override { result.emplace(call(decl)); }
            void Visit(SClassFuncDecl* decl) override { result.emplace(call(decl)); }
            void Visit(SClassCtorDecl* decl) override { result.emplace(call(decl)); }
            void Visit(SClassVarDecl* decl) override { result.emplace(call(decl)); }
        };

        Bridge bridge{caller};
        decl->Accept(bridge);
        return *bridge.result;
    }
}

SYNTAX_API JsonItem ToJson(SClassMemberDecl* decl);

class SStructMemberDeclVisitor
{
public:
    virtual ~SStructMemberDeclVisitor() = default;
    virtual void Visit(SClassDecl* decl) = 0;
    virtual void Visit(SStructDecl* decl) = 0;
    virtual void Visit(SEnumDecl* decl) = 0;
    virtual void Visit(SStructFuncDecl* decl) = 0;
    virtual void Visit(SStructCtorDecl* decl) = 0;
    virtual void Visit(SStructDtorDecl* decl) = 0;
    virtual void Visit(SStructVarDecl* decl) = 0;
};

class SStructMemberDecl : virtual public SSyntax
{
public:
    SStructMemberDecl() = default;
    SStructMemberDecl(const SStructMemberDecl&) = delete;
    SStructMemberDecl(SStructMemberDecl&&) = default;
    virtual ~SStructMemberDecl() { }
    SStructMemberDecl& operator=(const SStructMemberDecl& other) = delete;
    SStructMemberDecl& operator=(SStructMemberDecl&& other) noexcept = default;
    virtual void Accept(SStructMemberDeclVisitor& visitor) = 0;
};

template<class TFrom, class TVisitor>
concept SStructMemberDeclConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;
// ResultType은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept SStructMemberDeclVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<SClassDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SStructMemberDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStructDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SStructMemberDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SEnumDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SStructMemberDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStructFuncDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SStructMemberDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStructCtorDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SStructMemberDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStructDtorDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SStructMemberDeclConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStructVarDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SStructMemberDeclConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires SStructMemberDeclVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, SStructMemberDecl* decl, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : SStructMemberDeclVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(SClassDecl* decl) override { call(decl); }
            void Visit(SStructDecl* decl) override { call(decl); }
            void Visit(SEnumDecl* decl) override { call(decl); }
            void Visit(SStructFuncDecl* decl) override { call(decl); }
            void Visit(SStructCtorDecl* decl) override { call(decl); }
            void Visit(SStructDtorDecl* decl) override { call(decl); }
            void Visit(SStructVarDecl* decl) override { call(decl); }
        };

        Bridge bridge{caller};
        decl->Accept(bridge);
    }
    else
    {
        struct Bridge : SStructMemberDeclVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(SClassDecl* decl) override { result.emplace(call(decl)); }
            void Visit(SStructDecl* decl) override { result.emplace(call(decl)); }
            void Visit(SEnumDecl* decl) override { result.emplace(call(decl)); }
            void Visit(SStructFuncDecl* decl) override { result.emplace(call(decl)); }
            void Visit(SStructCtorDecl* decl) override { result.emplace(call(decl)); }
            void Visit(SStructDtorDecl* decl) override { result.emplace(call(decl)); }
            void Visit(SStructVarDecl* decl) override { result.emplace(call(decl)); }
        };

        Bridge bridge{caller};
        decl->Accept(bridge);
        return *bridge.result;
    }
}

SYNTAX_API JsonItem ToJson(SStructMemberDecl* decl);

class SNamespaceDeclElementVisitor
{
public:
    virtual ~SNamespaceDeclElementVisitor() = default;
    virtual void Visit(SGlobalFuncDecl* elem) = 0;
    virtual void Visit(SNamespaceDecl* elem) = 0;
    virtual void Visit(SClassDecl* elem) = 0;
    virtual void Visit(SStructDecl* elem) = 0;
    virtual void Visit(SEnumDecl* elem) = 0;
};

class SNamespaceDeclElement : virtual public SSyntax
{
public:
    SNamespaceDeclElement() = default;
    SNamespaceDeclElement(const SNamespaceDeclElement&) = delete;
    SNamespaceDeclElement(SNamespaceDeclElement&&) = default;
    virtual ~SNamespaceDeclElement() { }
    SNamespaceDeclElement& operator=(const SNamespaceDeclElement& other) = delete;
    SNamespaceDeclElement& operator=(SNamespaceDeclElement&& other) noexcept = default;
    virtual void Accept(SNamespaceDeclElementVisitor& visitor) = 0;
};

template<class TFrom, class TVisitor>
concept SNamespaceDeclElementConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;
// ResultType은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept SNamespaceDeclElementVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<SGlobalFuncDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SNamespaceDeclElementConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SNamespaceDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SNamespaceDeclElementConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SClassDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SNamespaceDeclElementConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStructDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SNamespaceDeclElementConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SEnumDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SNamespaceDeclElementConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires SNamespaceDeclElementVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, SNamespaceDeclElement* elem, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : SNamespaceDeclElementVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(SGlobalFuncDecl* elem) override { call(elem); }
            void Visit(SNamespaceDecl* elem) override { call(elem); }
            void Visit(SClassDecl* elem) override { call(elem); }
            void Visit(SStructDecl* elem) override { call(elem); }
            void Visit(SEnumDecl* elem) override { call(elem); }
        };

        Bridge bridge{caller};
        elem->Accept(bridge);
    }
    else
    {
        struct Bridge : SNamespaceDeclElementVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(SGlobalFuncDecl* elem) override { result.emplace(call(elem)); }
            void Visit(SNamespaceDecl* elem) override { result.emplace(call(elem)); }
            void Visit(SClassDecl* elem) override { result.emplace(call(elem)); }
            void Visit(SStructDecl* elem) override { result.emplace(call(elem)); }
            void Visit(SEnumDecl* elem) override { result.emplace(call(elem)); }
        };

        Bridge bridge{caller};
        elem->Accept(bridge);
        return *bridge.result;
    }
}

SYNTAX_API JsonItem ToJson(SNamespaceDeclElement* elem);

class SScriptElementVisitor
{
public:
    virtual ~SScriptElementVisitor() = default;
    virtual void Visit(SNamespaceDecl* elem) = 0;
    virtual void Visit(SGlobalFuncDecl* elem) = 0;
    virtual void Visit(SClassDecl* elem) = 0;
    virtual void Visit(SStructDecl* elem) = 0;
    virtual void Visit(SEnumDecl* elem) = 0;
};

class SScriptElement : virtual public SSyntax
{
public:
    SScriptElement() = default;
    SScriptElement(const SScriptElement&) = delete;
    SScriptElement(SScriptElement&&) = default;
    virtual ~SScriptElement() { }
    SScriptElement& operator=(const SScriptElement& other) = delete;
    SScriptElement& operator=(SScriptElement&& other) noexcept = default;
    virtual void Accept(SScriptElementVisitor& visitor) = 0;
};

template<class TFrom, class TVisitor>
concept SScriptElementConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;
// ResultType은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept SScriptElementVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<SNamespaceDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SScriptElementConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SGlobalFuncDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SScriptElementConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SClassDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SScriptElementConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SStructDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SScriptElementConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SEnumDecl*>(), std::forward<TVisitorArgs>(args)...) } -> SScriptElementConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires SScriptElementVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, SScriptElement* elem, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : SScriptElementVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(SNamespaceDecl* elem) override { call(elem); }
            void Visit(SGlobalFuncDecl* elem) override { call(elem); }
            void Visit(SClassDecl* elem) override { call(elem); }
            void Visit(SStructDecl* elem) override { call(elem); }
            void Visit(SEnumDecl* elem) override { call(elem); }
        };

        Bridge bridge{caller};
        elem->Accept(bridge);
    }
    else
    {
        struct Bridge : SScriptElementVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(SNamespaceDecl* elem) override { result.emplace(call(elem)); }
            void Visit(SGlobalFuncDecl* elem) override { result.emplace(call(elem)); }
            void Visit(SClassDecl* elem) override { result.emplace(call(elem)); }
            void Visit(SStructDecl* elem) override { result.emplace(call(elem)); }
            void Visit(SEnumDecl* elem) override { result.emplace(call(elem)); }
        };

        Bridge bridge{caller};
        elem->Accept(bridge);
        return *bridge.result;
    }
}

SYNTAX_API JsonItem ToJson(SScriptElement* elem);

class SVarDeclTypeVisitor
{
public:
    virtual ~SVarDeclTypeVisitor() = default;
    virtual void Visit(SVarDeclType_Var* type) = 0;
    virtual void Visit(SVarDeclType_VarRef* type) = 0;
    virtual void Visit(SVarDeclType_Ref* type) = 0;
    virtual void Visit(SVarDeclType_Normal* type) = 0;
};

class SVarDeclType : virtual public SSyntax
{
public:
    SVarDeclType() = default;
    SVarDeclType(const SVarDeclType&) = delete;
    SVarDeclType(SVarDeclType&&) = default;
    virtual ~SVarDeclType() { }
    SVarDeclType& operator=(const SVarDeclType& other) = delete;
    SVarDeclType& operator=(SVarDeclType&& other) noexcept = default;
    virtual void Accept(SVarDeclTypeVisitor& visitor) = 0;
};

template<class TFrom, class TVisitor>
concept SVarDeclTypeConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;
// ResultType은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept SVarDeclTypeVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;
    { v.Visit(std::declval<SVarDeclType_Var*>(), std::forward<TVisitorArgs>(args)...) } -> SVarDeclTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SVarDeclType_VarRef*>(), std::forward<TVisitorArgs>(args)...) } -> SVarDeclTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SVarDeclType_Ref*>(), std::forward<TVisitorArgs>(args)...) } -> SVarDeclTypeConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<SVarDeclType_Normal*>(), std::forward<TVisitorArgs>(args)...) } -> SVarDeclTypeConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires SVarDeclTypeVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, SVarDeclType* type, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : SVarDeclTypeVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(SVarDeclType_Var* type) override { call(type); }
            void Visit(SVarDeclType_VarRef* type) override { call(type); }
            void Visit(SVarDeclType_Ref* type) override { call(type); }
            void Visit(SVarDeclType_Normal* type) override { call(type); }
        };

        Bridge bridge{caller};
        type->Accept(bridge);
    }
    else
    {
        struct Bridge : SVarDeclTypeVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(SVarDeclType_Var* type) override { result.emplace(call(type)); }
            void Visit(SVarDeclType_VarRef* type) override { result.emplace(call(type)); }
            void Visit(SVarDeclType_Ref* type) override { result.emplace(call(type)); }
            void Visit(SVarDeclType_Normal* type) override { result.emplace(call(type)); }
        };

        Bridge bridge{caller};
        type->Accept(bridge);
        return *bridge.result;
    }
}

SYNTAX_API JsonItem ToJson(SVarDeclType* type);

class SExp_Identifier
    : public SExp
{
public:
    std::string value;
    std::vector<STypeExp*> typeArgs;

    SYNTAX_API SExp_Identifier(std::string value, std::vector<STypeExp*> typeArgs);
    SExp_Identifier(std::string value) : SExp_Identifier(move(value), {}) { }
    SExp_Identifier(const SExp_Identifier&) = delete;
    SYNTAX_API SExp_Identifier(SExp_Identifier&&) noexcept;
    SYNTAX_API virtual ~SExp_Identifier();

    SExp_Identifier& operator=(const SExp_Identifier& other) = delete;
    SYNTAX_API SExp_Identifier& operator=(SExp_Identifier&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(this); }

};

class SExp_String
    : public SExp
{
public:
    std::vector<SStringExpElement*> elements;

    SYNTAX_API SExp_String(std::vector<SStringExpElement*> elements);
    SYNTAX_API SExp_String(std::string&& str, SFactory& factory);
    SExp_String(const SExp_String&) = delete;
    SYNTAX_API SExp_String(SExp_String&&) noexcept;
    SYNTAX_API virtual ~SExp_String();

    SExp_String& operator=(const SExp_String& other) = delete;
    SYNTAX_API SExp_String& operator=(SExp_String&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(this); }

};

class SExp_IntLiteral
    : public SExp
{
public:
    int value;

    SYNTAX_API SExp_IntLiteral(int value);
    SExp_IntLiteral(const SExp_IntLiteral&) = delete;
    SYNTAX_API SExp_IntLiteral(SExp_IntLiteral&&) noexcept;
    SYNTAX_API virtual ~SExp_IntLiteral();

    SExp_IntLiteral& operator=(const SExp_IntLiteral& other) = delete;
    SYNTAX_API SExp_IntLiteral& operator=(SExp_IntLiteral&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(this); }

};

class SExp_BoolLiteral
    : public SExp
{
public:
    bool value;

    SYNTAX_API SExp_BoolLiteral(bool value);
    SExp_BoolLiteral(const SExp_BoolLiteral&) = delete;
    SYNTAX_API SExp_BoolLiteral(SExp_BoolLiteral&&) noexcept;
    SYNTAX_API virtual ~SExp_BoolLiteral();

    SExp_BoolLiteral& operator=(const SExp_BoolLiteral& other) = delete;
    SYNTAX_API SExp_BoolLiteral& operator=(SExp_BoolLiteral&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(this); }

};

class SExp_NullLiteral
    : public SExp
{
public:
    SYNTAX_API SExp_NullLiteral();
    SExp_NullLiteral(const SExp_NullLiteral&) = delete;
    SYNTAX_API SExp_NullLiteral(SExp_NullLiteral&&) noexcept;
    SYNTAX_API virtual ~SExp_NullLiteral();

    SExp_NullLiteral& operator=(const SExp_NullLiteral& other) = delete;
    SYNTAX_API SExp_NullLiteral& operator=(SExp_NullLiteral&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(this); }

};

class SExp_List
    : public SExp
{
public:
    std::vector<SExp*> elements;

    SYNTAX_API SExp_List(std::vector<SExp*> elements);
    SExp_List(const SExp_List&) = delete;
    SYNTAX_API SExp_List(SExp_List&&) noexcept;
    SYNTAX_API virtual ~SExp_List();

    SExp_List& operator=(const SExp_List& other) = delete;
    SYNTAX_API SExp_List& operator=(SExp_List&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(this); }

};

class SExp_New
    : public SExp
{
public:
    STypeExp* type;
    SArguments* args;

    SYNTAX_API SExp_New(STypeExp* type, SArguments* args);
    SExp_New(const SExp_New&) = delete;
    SYNTAX_API SExp_New(SExp_New&&) noexcept;
    SYNTAX_API virtual ~SExp_New();

    SExp_New& operator=(const SExp_New& other) = delete;
    SYNTAX_API SExp_New& operator=(SExp_New&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(this); }

};

class SExp_BinaryOp
    : public SExp
{
public:
    SBinaryOpKind kind;
    SExp* operand0;
    SExp* operand1;

    SYNTAX_API SExp_BinaryOp(SBinaryOpKind kind, SExp* operand0, SExp* operand1);
    SExp_BinaryOp(const SExp_BinaryOp&) = delete;
    SYNTAX_API SExp_BinaryOp(SExp_BinaryOp&&) noexcept;
    SYNTAX_API virtual ~SExp_BinaryOp();

    SExp_BinaryOp& operator=(const SExp_BinaryOp& other) = delete;
    SYNTAX_API SExp_BinaryOp& operator=(SExp_BinaryOp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(this); }

};

class SExp_UnaryOp
    : public SExp
{
public:
    SUnaryOpKind kind;
    SExp* operand;

    SYNTAX_API SExp_UnaryOp(SUnaryOpKind kind, SExp* operand);
    SExp_UnaryOp(const SExp_UnaryOp&) = delete;
    SYNTAX_API SExp_UnaryOp(SExp_UnaryOp&&) noexcept;
    SYNTAX_API virtual ~SExp_UnaryOp();

    SExp_UnaryOp& operator=(const SExp_UnaryOp& other) = delete;
    SYNTAX_API SExp_UnaryOp& operator=(SExp_UnaryOp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(this); }

};

class SExp_Call
    : public SExp
{
public:
    SExp* callable;
    SArguments* args;

    SYNTAX_API SExp_Call(SExp* callable, SArguments* args);
    SExp_Call(const SExp_Call&) = delete;
    SYNTAX_API SExp_Call(SExp_Call&&) noexcept;
    SYNTAX_API virtual ~SExp_Call();

    SExp_Call& operator=(const SExp_Call& other) = delete;
    SYNTAX_API SExp_Call& operator=(SExp_Call&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(this); }

};

class SExp_Lambda
    : public SExp
{
public:
    std::vector<SLambdaExpParam> params;
    SLambdaExpBody* body;

    SYNTAX_API SExp_Lambda(std::vector<SLambdaExpParam> params, SLambdaExpBody* body);
    SExp_Lambda(const SExp_Lambda&) = delete;
    SYNTAX_API SExp_Lambda(SExp_Lambda&&) noexcept;
    SYNTAX_API virtual ~SExp_Lambda();

    SExp_Lambda& operator=(const SExp_Lambda& other) = delete;
    SYNTAX_API SExp_Lambda& operator=(SExp_Lambda&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(this); }

};

class SExp_Indexer
    : public SExp
{
public:
    SExp* obj;
    SExp* index;

    SYNTAX_API SExp_Indexer(SExp* obj, SExp* index);
    SExp_Indexer(const SExp_Indexer&) = delete;
    SYNTAX_API SExp_Indexer(SExp_Indexer&&) noexcept;
    SYNTAX_API virtual ~SExp_Indexer();

    SExp_Indexer& operator=(const SExp_Indexer& other) = delete;
    SYNTAX_API SExp_Indexer& operator=(SExp_Indexer&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(this); }

};

class SExp_Member
    : public SExp
{
public:
    SExp* base;
    std::string memberName;
    std::vector<STypeExp*> memberTypeArgs;

    SYNTAX_API SExp_Member(SExp* base, std::string memberName, std::vector<STypeExp*> memberTypeArgs);
    SYNTAX_API SExp_Member(SExp* base, std::string&& memberName);
    SExp_Member(const SExp_Member&) = delete;
    SYNTAX_API SExp_Member(SExp_Member&&) noexcept;
    SYNTAX_API virtual ~SExp_Member();

    SExp_Member& operator=(const SExp_Member& other) = delete;
    SYNTAX_API SExp_Member& operator=(SExp_Member&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(this); }

};

class SExp_Shared
    : public SExp
{
public:
    SExp* innerExp;

    SYNTAX_API SExp_Shared(SExp* innerExp);
    SExp_Shared(const SExp_Shared&) = delete;
    SYNTAX_API SExp_Shared(SExp_Shared&&) noexcept;
    SYNTAX_API virtual ~SExp_Shared();

    SExp_Shared& operator=(const SExp_Shared& other) = delete;
    SYNTAX_API SExp_Shared& operator=(SExp_Shared&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(this); }

};

class SExp_Is
    : public SExp
{
public:
    SExp* exp;
    STypeExp* type;

    SYNTAX_API SExp_Is(SExp* exp, STypeExp* type);
    SExp_Is(const SExp_Is&) = delete;
    SYNTAX_API SExp_Is(SExp_Is&&) noexcept;
    SYNTAX_API virtual ~SExp_Is();

    SExp_Is& operator=(const SExp_Is& other) = delete;
    SYNTAX_API SExp_Is& operator=(SExp_Is&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(this); }

};

class SExp_As
    : public SExp
{
public:
    SExp* exp;
    STypeExp* type;

    SYNTAX_API SExp_As(SExp* exp, STypeExp* type);
    SExp_As(const SExp_As&) = delete;
    SYNTAX_API SExp_As(SExp_As&&) noexcept;
    SYNTAX_API virtual ~SExp_As();

    SExp_As& operator=(const SExp_As& other) = delete;
    SYNTAX_API SExp_As& operator=(SExp_As&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SExpVisitor& visitor) override { visitor.Visit(this); }

};

class STypeExp_Id
    : public STypeExp
{
public:
    std::string memberName;
    std::vector<STypeExp*> typeArgs;

    SYNTAX_API STypeExp_Id(std::string memberName, std::vector<STypeExp*> typeArgs);
    SYNTAX_API STypeExp_Id(std::string&& memberName);
    STypeExp_Id(const STypeExp_Id&) = delete;
    SYNTAX_API STypeExp_Id(STypeExp_Id&&) noexcept;
    SYNTAX_API virtual ~STypeExp_Id();

    STypeExp_Id& operator=(const STypeExp_Id& other) = delete;
    SYNTAX_API STypeExp_Id& operator=(STypeExp_Id&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(this); }

};

class STypeExp_Member
    : public STypeExp
{
public:
    STypeExp* parentType;
    std::string memberName;
    std::vector<STypeExp*> typeArgs;

    SYNTAX_API STypeExp_Member(STypeExp* parentType, std::string memberName, std::vector<STypeExp*> typeArgs);
    STypeExp_Member(const STypeExp_Member&) = delete;
    SYNTAX_API STypeExp_Member(STypeExp_Member&&) noexcept;
    SYNTAX_API virtual ~STypeExp_Member();

    STypeExp_Member& operator=(const STypeExp_Member& other) = delete;
    SYNTAX_API STypeExp_Member& operator=(STypeExp_Member&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(this); }

};

class STypeExp_Nullable
    : public STypeExp
{
public:
    STypeExp* innerType;

    SYNTAX_API STypeExp_Nullable(STypeExp* innerType);
    STypeExp_Nullable(const STypeExp_Nullable&) = delete;
    SYNTAX_API STypeExp_Nullable(STypeExp_Nullable&&) noexcept;
    SYNTAX_API virtual ~STypeExp_Nullable();

    STypeExp_Nullable& operator=(const STypeExp_Nullable& other) = delete;
    SYNTAX_API STypeExp_Nullable& operator=(STypeExp_Nullable&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(this); }

};

class STypeExp_Shared
    : public STypeExp
{
public:
    STypeExp* innerType;

    SYNTAX_API STypeExp_Shared(STypeExp* innerType);
    STypeExp_Shared(const STypeExp_Shared&) = delete;
    SYNTAX_API STypeExp_Shared(STypeExp_Shared&&) noexcept;
    SYNTAX_API virtual ~STypeExp_Shared();

    STypeExp_Shared& operator=(const STypeExp_Shared& other) = delete;
    SYNTAX_API STypeExp_Shared& operator=(STypeExp_Shared&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(this); }

};

class STypeExp_Box
    : public STypeExp
{
public:
    STypeExp* innerType;

    SYNTAX_API STypeExp_Box(STypeExp* innerType);
    STypeExp_Box(const STypeExp_Box&) = delete;
    SYNTAX_API STypeExp_Box(STypeExp_Box&&) noexcept;
    SYNTAX_API virtual ~STypeExp_Box();

    STypeExp_Box& operator=(const STypeExp_Box& other) = delete;
    SYNTAX_API STypeExp_Box& operator=(STypeExp_Box&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(this); }

};

class STypeExp_Ptr
    : public STypeExp
{
public:
    STypeExp* innerType;

    SYNTAX_API STypeExp_Ptr(STypeExp* innerType);
    STypeExp_Ptr(const STypeExp_Ptr&) = delete;
    SYNTAX_API STypeExp_Ptr(STypeExp_Ptr&&) noexcept;
    SYNTAX_API virtual ~STypeExp_Ptr();

    STypeExp_Ptr& operator=(const STypeExp_Ptr& other) = delete;
    SYNTAX_API STypeExp_Ptr& operator=(STypeExp_Ptr&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(this); }

};

class STypeExp_Local
    : public STypeExp
{
public:
    STypeExp* innerType;

    SYNTAX_API STypeExp_Local(STypeExp* innerType);
    STypeExp_Local(const STypeExp_Local&) = delete;
    SYNTAX_API STypeExp_Local(STypeExp_Local&&) noexcept;
    SYNTAX_API virtual ~STypeExp_Local();

    STypeExp_Local& operator=(const STypeExp_Local& other) = delete;
    SYNTAX_API STypeExp_Local& operator=(STypeExp_Local&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(STypeExpVisitor& visitor) override { visitor.Visit(this); }

};

enum class SVarDeclType_VarKind
{
    Normal,
    Ptr,
    Nullable,
    Shared,
    Box,
    Local,
};

inline JsonItem ToJson(SVarDeclType_VarKind& arg)
{
    switch(arg)
    {
    case SVarDeclType_VarKind::Normal: return JsonString("Normal");
    case SVarDeclType_VarKind::Ptr: return JsonString("Ptr");
    case SVarDeclType_VarKind::Nullable: return JsonString("Nullable");
    case SVarDeclType_VarKind::Shared: return JsonString("Shared");
    case SVarDeclType_VarKind::Box: return JsonString("Box");
    case SVarDeclType_VarKind::Local: return JsonString("Local");
    }
    unreachable();
}

class SVarDeclType_Var
    : public SVarDeclType
{
public:
    SVarDeclType_VarKind kind;

    SYNTAX_API SVarDeclType_Var(SVarDeclType_VarKind kind);
    SVarDeclType_Var(const SVarDeclType_Var&) = delete;
    SYNTAX_API SVarDeclType_Var(SVarDeclType_Var&&) noexcept;
    SYNTAX_API virtual ~SVarDeclType_Var();

    SVarDeclType_Var& operator=(const SVarDeclType_Var& other) = delete;
    SYNTAX_API SVarDeclType_Var& operator=(SVarDeclType_Var&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SVarDeclTypeVisitor& visitor) override { visitor.Visit(this); }

};

class SVarDeclType_VarRef
    : public SVarDeclType
{
public:
    SYNTAX_API SVarDeclType_VarRef();
    SVarDeclType_VarRef(const SVarDeclType_VarRef&) = delete;
    SYNTAX_API SVarDeclType_VarRef(SVarDeclType_VarRef&&) noexcept;
    SYNTAX_API virtual ~SVarDeclType_VarRef();

    SVarDeclType_VarRef& operator=(const SVarDeclType_VarRef& other) = delete;
    SYNTAX_API SVarDeclType_VarRef& operator=(SVarDeclType_VarRef&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SVarDeclTypeVisitor& visitor) override { visitor.Visit(this); }

};

class SVarDeclType_Ref
    : public SVarDeclType
{
public:
    STypeExp* typeExp;

    SYNTAX_API SVarDeclType_Ref(STypeExp* typeExp);
    SVarDeclType_Ref(const SVarDeclType_Ref&) = delete;
    SYNTAX_API SVarDeclType_Ref(SVarDeclType_Ref&&) noexcept;
    SYNTAX_API virtual ~SVarDeclType_Ref();

    SVarDeclType_Ref& operator=(const SVarDeclType_Ref& other) = delete;
    SYNTAX_API SVarDeclType_Ref& operator=(SVarDeclType_Ref&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SVarDeclTypeVisitor& visitor) override { visitor.Visit(this); }

};

class SVarDeclType_Normal
    : public SVarDeclType
{
public:
    STypeExp* typeExp;

    SYNTAX_API SVarDeclType_Normal(STypeExp* typeExp);
    SVarDeclType_Normal(const SVarDeclType_Normal&) = delete;
    SYNTAX_API SVarDeclType_Normal(SVarDeclType_Normal&&) noexcept;
    SYNTAX_API virtual ~SVarDeclType_Normal();

    SVarDeclType_Normal& operator=(const SVarDeclType_Normal& other) = delete;
    SYNTAX_API SVarDeclType_Normal& operator=(SVarDeclType_Normal&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SVarDeclTypeVisitor& visitor) override { visitor.Visit(this); }

};

class SStringExpElement_Text
    : public SStringExpElement
{
public:
    std::string text;

    SYNTAX_API SStringExpElement_Text(std::string text);
    SStringExpElement_Text(const SStringExpElement_Text&) = delete;
    SYNTAX_API SStringExpElement_Text(SStringExpElement_Text&&) noexcept;
    SYNTAX_API virtual ~SStringExpElement_Text();

    SStringExpElement_Text& operator=(const SStringExpElement_Text& other) = delete;
    SYNTAX_API SStringExpElement_Text& operator=(SStringExpElement_Text&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStringExpElementVisitor& visitor) override { visitor.Visit(this); }

};

class SStringExpElement_Exp
    : public SStringExpElement
{
public:
    SExp* exp;

    SYNTAX_API SStringExpElement_Exp(SExp* exp);
    SStringExpElement_Exp(const SStringExpElement_Exp&) = delete;
    SYNTAX_API SStringExpElement_Exp(SStringExpElement_Exp&&) noexcept;
    SYNTAX_API virtual ~SStringExpElement_Exp();

    SStringExpElement_Exp& operator=(const SStringExpElement_Exp& other) = delete;
    SYNTAX_API SStringExpElement_Exp& operator=(SStringExpElement_Exp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStringExpElementVisitor& visitor) override { visitor.Visit(this); }

};

class SLambdaExpBody_Stmts
    : public SLambdaExpBody
{
public:
    std::vector<SStmt*> stmts;

    SYNTAX_API SLambdaExpBody_Stmts(std::vector<SStmt*> stmts);
    SLambdaExpBody_Stmts(const SLambdaExpBody_Stmts&) = delete;
    SYNTAX_API SLambdaExpBody_Stmts(SLambdaExpBody_Stmts&&) noexcept;
    SYNTAX_API virtual ~SLambdaExpBody_Stmts();

    SLambdaExpBody_Stmts& operator=(const SLambdaExpBody_Stmts& other) = delete;
    SYNTAX_API SLambdaExpBody_Stmts& operator=(SLambdaExpBody_Stmts&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SLambdaExpBodyVisitor& visitor) override { visitor.Visit(this); }

};

class SLambdaExpBody_Exp
    : public SLambdaExpBody
{
public:
    SExp* exp;

    SYNTAX_API SLambdaExpBody_Exp(SExp* exp);
    SLambdaExpBody_Exp(const SLambdaExpBody_Exp&) = delete;
    SYNTAX_API SLambdaExpBody_Exp(SLambdaExpBody_Exp&&) noexcept;
    SYNTAX_API virtual ~SLambdaExpBody_Exp();

    SLambdaExpBody_Exp& operator=(const SLambdaExpBody_Exp& other) = delete;
    SYNTAX_API SLambdaExpBody_Exp& operator=(SLambdaExpBody_Exp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SLambdaExpBodyVisitor& visitor) override { visitor.Visit(this); }

};

class SEmbeddableStmt_Single
    : public SEmbeddableStmt
{
public:
    SStmt* stmt;

    SYNTAX_API SEmbeddableStmt_Single(SStmt* stmt);
    SEmbeddableStmt_Single(const SEmbeddableStmt_Single&) = delete;
    SYNTAX_API SEmbeddableStmt_Single(SEmbeddableStmt_Single&&) noexcept;
    SYNTAX_API virtual ~SEmbeddableStmt_Single();

    SEmbeddableStmt_Single& operator=(const SEmbeddableStmt_Single& other) = delete;
    SYNTAX_API SEmbeddableStmt_Single& operator=(SEmbeddableStmt_Single&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SEmbeddableStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SEmbeddableStmt_Block
    : public SEmbeddableStmt
{
public:
    std::vector<SStmt*> stmts;

    SYNTAX_API SEmbeddableStmt_Block(std::vector<SStmt*> stmts);
    SEmbeddableStmt_Block(const SEmbeddableStmt_Block&) = delete;
    SYNTAX_API SEmbeddableStmt_Block(SEmbeddableStmt_Block&&) noexcept;
    SYNTAX_API virtual ~SEmbeddableStmt_Block();

    SEmbeddableStmt_Block& operator=(const SEmbeddableStmt_Block& other) = delete;
    SYNTAX_API SEmbeddableStmt_Block& operator=(SEmbeddableStmt_Block&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SEmbeddableStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SForStmtInitializer_Exp
    : public SForStmtInitializer
{
public:
    SExp* exp;

    SYNTAX_API SForStmtInitializer_Exp(SExp* exp);
    SForStmtInitializer_Exp(const SForStmtInitializer_Exp&) = delete;
    SYNTAX_API SForStmtInitializer_Exp(SForStmtInitializer_Exp&&) noexcept;
    SYNTAX_API virtual ~SForStmtInitializer_Exp();

    SForStmtInitializer_Exp& operator=(const SForStmtInitializer_Exp& other) = delete;
    SYNTAX_API SForStmtInitializer_Exp& operator=(SForStmtInitializer_Exp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SForStmtInitializerVisitor& visitor) override { visitor.Visit(this); }

};

class SForStmtInitializer_VarDecl
    : public SForStmtInitializer
{
public:
    SVarDecl varDecl;

    SYNTAX_API SForStmtInitializer_VarDecl(SVarDecl varDecl);
    SForStmtInitializer_VarDecl(const SForStmtInitializer_VarDecl&) = delete;
    SYNTAX_API SForStmtInitializer_VarDecl(SForStmtInitializer_VarDecl&&) noexcept;
    SYNTAX_API virtual ~SForStmtInitializer_VarDecl();

    SForStmtInitializer_VarDecl& operator=(const SForStmtInitializer_VarDecl& other) = delete;
    SYNTAX_API SForStmtInitializer_VarDecl& operator=(SForStmtInitializer_VarDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SForStmtInitializerVisitor& visitor) override { visitor.Visit(this); }

};

class SStmt_Command
    : public SStmt
{
public:
    std::vector<SExp_String*> commands;

    SYNTAX_API SStmt_Command(std::vector<SExp_String*> commands);
    SStmt_Command(const SStmt_Command&) = delete;
    SYNTAX_API SStmt_Command(SStmt_Command&&) noexcept;
    SYNTAX_API virtual ~SStmt_Command();

    SStmt_Command& operator=(const SStmt_Command& other) = delete;
    SYNTAX_API SStmt_Command& operator=(SStmt_Command&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SStmt_VarDecl
    : public SStmt
{
public:
    SVarDecl varDecl;

    SYNTAX_API SStmt_VarDecl(SVarDecl varDecl);
    SStmt_VarDecl(const SStmt_VarDecl&) = delete;
    SYNTAX_API SStmt_VarDecl(SStmt_VarDecl&&) noexcept;
    SYNTAX_API virtual ~SStmt_VarDecl();

    SStmt_VarDecl& operator=(const SStmt_VarDecl& other) = delete;
    SYNTAX_API SStmt_VarDecl& operator=(SStmt_VarDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SStmt_Continue
    : public SStmt
{
public:
    SYNTAX_API SStmt_Continue();
    SStmt_Continue(const SStmt_Continue&) = delete;
    SYNTAX_API SStmt_Continue(SStmt_Continue&&) noexcept;
    SYNTAX_API virtual ~SStmt_Continue();

    SStmt_Continue& operator=(const SStmt_Continue& other) = delete;
    SYNTAX_API SStmt_Continue& operator=(SStmt_Continue&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SStmt_Break
    : public SStmt
{
public:
    SYNTAX_API SStmt_Break();
    SStmt_Break(const SStmt_Break&) = delete;
    SYNTAX_API SStmt_Break(SStmt_Break&&) noexcept;
    SYNTAX_API virtual ~SStmt_Break();

    SStmt_Break& operator=(const SStmt_Break& other) = delete;
    SYNTAX_API SStmt_Break& operator=(SStmt_Break&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SStmt_Block
    : public SStmt
{
public:
    std::vector<SStmt*> stmts;

    SYNTAX_API SStmt_Block(std::vector<SStmt*> stmts);
    SStmt_Block(const SStmt_Block&) = delete;
    SYNTAX_API SStmt_Block(SStmt_Block&&) noexcept;
    SYNTAX_API virtual ~SStmt_Block();

    SStmt_Block& operator=(const SStmt_Block& other) = delete;
    SYNTAX_API SStmt_Block& operator=(SStmt_Block&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SStmt_Blank
    : public SStmt
{
public:
    SYNTAX_API SStmt_Blank();
    SStmt_Blank(const SStmt_Blank&) = delete;
    SYNTAX_API SStmt_Blank(SStmt_Blank&&) noexcept;
    SYNTAX_API virtual ~SStmt_Blank();

    SStmt_Blank& operator=(const SStmt_Blank& other) = delete;
    SYNTAX_API SStmt_Blank& operator=(SStmt_Blank&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SStmt_Task
    : public SStmt
{
public:
    std::vector<SStmt*> body;

    SYNTAX_API SStmt_Task(std::vector<SStmt*> body);
    SStmt_Task(const SStmt_Task&) = delete;
    SYNTAX_API SStmt_Task(SStmt_Task&&) noexcept;
    SYNTAX_API virtual ~SStmt_Task();

    SStmt_Task& operator=(const SStmt_Task& other) = delete;
    SYNTAX_API SStmt_Task& operator=(SStmt_Task&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SStmt_Await
    : public SStmt
{
public:
    std::vector<SStmt*> body;

    SYNTAX_API SStmt_Await(std::vector<SStmt*> body);
    SStmt_Await(const SStmt_Await&) = delete;
    SYNTAX_API SStmt_Await(SStmt_Await&&) noexcept;
    SYNTAX_API virtual ~SStmt_Await();

    SStmt_Await& operator=(const SStmt_Await& other) = delete;
    SYNTAX_API SStmt_Await& operator=(SStmt_Await&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SStmt_Async
    : public SStmt
{
public:
    std::vector<SStmt*> body;

    SYNTAX_API SStmt_Async(std::vector<SStmt*> body);
    SStmt_Async(const SStmt_Async&) = delete;
    SYNTAX_API SStmt_Async(SStmt_Async&&) noexcept;
    SYNTAX_API virtual ~SStmt_Async();

    SStmt_Async& operator=(const SStmt_Async& other) = delete;
    SYNTAX_API SStmt_Async& operator=(SStmt_Async&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SStmt_Directive
    : public SStmt
{
public:
    std::string memberName;
    std::vector<SExp*> args;

    SYNTAX_API SStmt_Directive(std::string memberName, std::vector<SExp*> args);
    SStmt_Directive(const SStmt_Directive&) = delete;
    SYNTAX_API SStmt_Directive(SStmt_Directive&&) noexcept;
    SYNTAX_API virtual ~SStmt_Directive();

    SStmt_Directive& operator=(const SStmt_Directive& other) = delete;
    SYNTAX_API SStmt_Directive& operator=(SStmt_Directive&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SStmt_If
    : public SStmt
{
public:
    SExp* cond;
    SEmbeddableStmt* body;
    SEmbeddableStmt* elseBody;

    SYNTAX_API SStmt_If(SExp* cond, SEmbeddableStmt* body, SEmbeddableStmt* elseBody);
    SStmt_If(const SStmt_If&) = delete;
    SYNTAX_API SStmt_If(SStmt_If&&) noexcept;
    SYNTAX_API virtual ~SStmt_If();

    SStmt_If& operator=(const SStmt_If& other) = delete;
    SYNTAX_API SStmt_If& operator=(SStmt_If&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SStmt_IfBind
    : public SStmt
{
public:
    STypeExp* testType;
    std::string varName;
    SExp* exp;
    SEmbeddableStmt* body;
    SEmbeddableStmt* elseBody;

    SYNTAX_API SStmt_IfBind(STypeExp* testType, std::string varName, SExp* exp, SEmbeddableStmt* body, SEmbeddableStmt* elseBody);
    SStmt_IfBind(const SStmt_IfBind&) = delete;
    SYNTAX_API SStmt_IfBind(SStmt_IfBind&&) noexcept;
    SYNTAX_API virtual ~SStmt_IfBind();

    SStmt_IfBind& operator=(const SStmt_IfBind& other) = delete;
    SYNTAX_API SStmt_IfBind& operator=(SStmt_IfBind&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SStmt_For
    : public SStmt
{
public:
    SForStmtInitializer* initializer;
    SExp* cond;
    SExp* cont;
    SEmbeddableStmt* body;

    SYNTAX_API SStmt_For(SForStmtInitializer* initializer, SExp* cond, SExp* cont, SEmbeddableStmt* body);
    SStmt_For(const SStmt_For&) = delete;
    SYNTAX_API SStmt_For(SStmt_For&&) noexcept;
    SYNTAX_API virtual ~SStmt_For();

    SStmt_For& operator=(const SStmt_For& other) = delete;
    SYNTAX_API SStmt_For& operator=(SStmt_For&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SStmt_Return
    : public SStmt
{
public:
    SExp* value;

    SYNTAX_API SStmt_Return(SExp* value);
    SStmt_Return(const SStmt_Return&) = delete;
    SYNTAX_API SStmt_Return(SStmt_Return&&) noexcept;
    SYNTAX_API virtual ~SStmt_Return();

    SStmt_Return& operator=(const SStmt_Return& other) = delete;
    SYNTAX_API SStmt_Return& operator=(SStmt_Return&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SStmt_Exp
    : public SStmt
{
public:
    SExp* exp;

    SYNTAX_API SStmt_Exp(SExp* exp);
    SStmt_Exp(const SStmt_Exp&) = delete;
    SYNTAX_API SStmt_Exp(SStmt_Exp&&) noexcept;
    SYNTAX_API virtual ~SStmt_Exp();

    SStmt_Exp& operator=(const SStmt_Exp& other) = delete;
    SYNTAX_API SStmt_Exp& operator=(SStmt_Exp&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SStmt_Foreach
    : public SStmt
{
public:
    STypeExp* type;
    std::string varName;
    SExp* enumerable;
    SEmbeddableStmt* body;

    SYNTAX_API SStmt_Foreach(STypeExp* type, std::string varName, SExp* enumerable, SEmbeddableStmt* body);
    SStmt_Foreach(const SStmt_Foreach&) = delete;
    SYNTAX_API SStmt_Foreach(SStmt_Foreach&&) noexcept;
    SYNTAX_API virtual ~SStmt_Foreach();

    SStmt_Foreach& operator=(const SStmt_Foreach& other) = delete;
    SYNTAX_API SStmt_Foreach& operator=(SStmt_Foreach&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SStmt_Yield
    : public SStmt
{
public:
    SExp* value;

    SYNTAX_API SStmt_Yield(SExp* value);
    SStmt_Yield(const SStmt_Yield&) = delete;
    SYNTAX_API SStmt_Yield(SStmt_Yield&&) noexcept;
    SYNTAX_API virtual ~SStmt_Yield();

    SStmt_Yield& operator=(const SStmt_Yield& other) = delete;
    SYNTAX_API SStmt_Yield& operator=(SStmt_Yield&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStmtVisitor& visitor) override { visitor.Visit(this); }

};

class SGlobalFuncDecl
    : public SNamespaceDeclElement
    , public SScriptElement
{
public:
    std::optional<SAccessModifier> accessModifier;
    bool bSequence;
    STypeExp* retType;
    std::string memberName;
    std::vector<STypeParam> typeParams;
    std::vector<SFuncParam> parameters;
    std::vector<SStmt*> body;

    SYNTAX_API SGlobalFuncDecl(std::optional<SAccessModifier> accessModifier, bool bSequence, STypeExp* retType, std::string memberName, std::vector<STypeParam> typeParams, std::vector<SFuncParam> parameters, std::vector<SStmt*> body);
    SGlobalFuncDecl(const SGlobalFuncDecl&) = delete;
    SYNTAX_API SGlobalFuncDecl(SGlobalFuncDecl&&) noexcept;
    SYNTAX_API virtual ~SGlobalFuncDecl();

    SGlobalFuncDecl& operator=(const SGlobalFuncDecl& other) = delete;
    SYNTAX_API SGlobalFuncDecl& operator=(SGlobalFuncDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SNamespaceDeclElementVisitor& visitor) override { visitor.Visit(this); }
    void Accept(SScriptElementVisitor& visitor) override { visitor.Visit(this); }

};

class SClassDecl
    : public SClassMemberDecl
    , public SStructMemberDecl
    , public SNamespaceDeclElement
    , public SScriptElement
{
public:
    std::optional<SAccessModifier> accessModifier;
    std::string memberName;
    std::vector<STypeParam> typeParams;
    std::vector<STypeExp*> baseTypes;
    std::vector<SClassMemberDecl*> memberDecls;

    SYNTAX_API SClassDecl(std::optional<SAccessModifier> accessModifier, std::string memberName, std::vector<STypeParam> typeParams, std::vector<STypeExp*> baseTypes, std::vector<SClassMemberDecl*> memberDecls);
    SClassDecl(const SClassDecl&) = delete;
    SYNTAX_API SClassDecl(SClassDecl&&) noexcept;
    SYNTAX_API virtual ~SClassDecl();

    SClassDecl& operator=(const SClassDecl& other) = delete;
    SYNTAX_API SClassDecl& operator=(SClassDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SClassMemberDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(SNamespaceDeclElementVisitor& visitor) override { visitor.Visit(this); }
    void Accept(SScriptElementVisitor& visitor) override { visitor.Visit(this); }

};

class SClassFuncDecl
    : public SClassMemberDecl
{
public:
    std::optional<SAccessModifier> accessModifier;
    bool bStatic;
    bool bSequence;
    STypeExp* retType;
    std::string memberName;
    std::vector<STypeParam> typeParams;
    std::vector<SFuncParam> parameters;
    std::vector<SStmt*> body;

    SYNTAX_API SClassFuncDecl(std::optional<SAccessModifier> accessModifier, bool bStatic, bool bSequence, STypeExp* retType, std::string memberName, std::vector<STypeParam> typeParams, std::vector<SFuncParam> parameters, std::vector<SStmt*> body);
    SClassFuncDecl(const SClassFuncDecl&) = delete;
    SYNTAX_API SClassFuncDecl(SClassFuncDecl&&) noexcept;
    SYNTAX_API virtual ~SClassFuncDecl();

    SClassFuncDecl& operator=(const SClassFuncDecl& other) = delete;
    SYNTAX_API SClassFuncDecl& operator=(SClassFuncDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SClassMemberDeclVisitor& visitor) override { visitor.Visit(this); }

};

class SClassCtorDecl
    : public SClassMemberDecl
{
public:
    std::optional<SAccessModifier> accessModifier;
    std::vector<SFuncParam> parameters;
    SArguments* baseArgs;
    std::vector<SStmt*> body;

    SYNTAX_API SClassCtorDecl(std::optional<SAccessModifier> accessModifier, std::vector<SFuncParam> parameters, SArguments* baseArgs, std::vector<SStmt*> body);
    SClassCtorDecl(const SClassCtorDecl&) = delete;
    SYNTAX_API SClassCtorDecl(SClassCtorDecl&&) noexcept;
    SYNTAX_API virtual ~SClassCtorDecl();

    SClassCtorDecl& operator=(const SClassCtorDecl& other) = delete;
    SYNTAX_API SClassCtorDecl& operator=(SClassCtorDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SClassMemberDeclVisitor& visitor) override { visitor.Visit(this); }

};

class SClassVarDecl
    : public SClassMemberDecl
{
public:
    std::optional<SAccessModifier> accessModifier;
    STypeExp* varType;
    std::vector<std::string> varNames;

    SYNTAX_API SClassVarDecl(std::optional<SAccessModifier> accessModifier, STypeExp* varType, std::vector<std::string> varNames);
    SClassVarDecl(const SClassVarDecl&) = delete;
    SYNTAX_API SClassVarDecl(SClassVarDecl&&) noexcept;
    SYNTAX_API virtual ~SClassVarDecl();

    SClassVarDecl& operator=(const SClassVarDecl& other) = delete;
    SYNTAX_API SClassVarDecl& operator=(SClassVarDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SClassMemberDeclVisitor& visitor) override { visitor.Visit(this); }

};

class SStructDecl
    : public SClassMemberDecl
    , public SStructMemberDecl
    , public SNamespaceDeclElement
    , public SScriptElement
{
public:
    std::optional<SAccessModifier> accessModifier;
    std::string memberName;
    std::vector<STypeParam> typeParams;
    std::vector<STypeExp*> baseTypes;
    std::vector<SStructMemberDecl*> memberDecls;

    SYNTAX_API SStructDecl(std::optional<SAccessModifier> accessModifier, std::string memberName, std::vector<STypeParam> typeParams, std::vector<STypeExp*> baseTypes, std::vector<SStructMemberDecl*> memberDecls);
    SStructDecl(const SStructDecl&) = delete;
    SYNTAX_API SStructDecl(SStructDecl&&) noexcept;
    SYNTAX_API virtual ~SStructDecl();

    SStructDecl& operator=(const SStructDecl& other) = delete;
    SYNTAX_API SStructDecl& operator=(SStructDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SClassMemberDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(SNamespaceDeclElementVisitor& visitor) override { visitor.Visit(this); }
    void Accept(SScriptElementVisitor& visitor) override { visitor.Visit(this); }

};

class SStructFuncDecl
    : public SStructMemberDecl
{
public:
    std::optional<SAccessModifier> accessModifier;
    bool bStatic;
    bool bSequence;
    STypeExp* retType;
    std::string memberName;
    std::vector<STypeParam> typeParams;
    std::vector<SFuncParam> parameters;
    std::vector<SStmt*> body;

    SYNTAX_API SStructFuncDecl(std::optional<SAccessModifier> accessModifier, bool bStatic, bool bSequence, STypeExp* retType, std::string memberName, std::vector<STypeParam> typeParams, std::vector<SFuncParam> parameters, std::vector<SStmt*> body);
    SStructFuncDecl(const SStructFuncDecl&) = delete;
    SYNTAX_API SStructFuncDecl(SStructFuncDecl&&) noexcept;
    SYNTAX_API virtual ~SStructFuncDecl();

    SStructFuncDecl& operator=(const SStructFuncDecl& other) = delete;
    SYNTAX_API SStructFuncDecl& operator=(SStructFuncDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(this); }

};

class SStructCtorDecl
    : public SStructMemberDecl
{
public:
    std::optional<SAccessModifier> accessModifier;
    std::vector<SFuncParam> parameters;
    std::vector<SStmt*> body;

    SYNTAX_API SStructCtorDecl(std::optional<SAccessModifier> accessModifier, std::vector<SFuncParam> parameters, std::vector<SStmt*> body);
    SStructCtorDecl(const SStructCtorDecl&) = delete;
    SYNTAX_API SStructCtorDecl(SStructCtorDecl&&) noexcept;
    SYNTAX_API virtual ~SStructCtorDecl();

    SStructCtorDecl& operator=(const SStructCtorDecl& other) = delete;
    SYNTAX_API SStructCtorDecl& operator=(SStructCtorDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(this); }

};

class SStructDtorDecl
    : public SStructMemberDecl
{
public:
    std::optional<SAccessModifier> accessModifier;
    std::vector<SStmt*> body;

    SYNTAX_API SStructDtorDecl(std::optional<SAccessModifier> accessModifier, std::vector<SStmt*> body);
    SStructDtorDecl(const SStructDtorDecl&) = delete;
    SYNTAX_API SStructDtorDecl(SStructDtorDecl&&) noexcept;
    SYNTAX_API virtual ~SStructDtorDecl();

    SStructDtorDecl& operator=(const SStructDtorDecl& other) = delete;
    SYNTAX_API SStructDtorDecl& operator=(SStructDtorDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(this); }

};

class SStructVarDecl
    : public SStructMemberDecl
{
public:
    std::optional<SAccessModifier> accessModifier;
    STypeExp* varType;
    std::vector<std::string> varNames;

    SYNTAX_API SStructVarDecl(std::optional<SAccessModifier> accessModifier, STypeExp* varType, std::vector<std::string> varNames);
    SStructVarDecl(const SStructVarDecl&) = delete;
    SYNTAX_API SStructVarDecl(SStructVarDecl&&) noexcept;
    SYNTAX_API virtual ~SStructVarDecl();

    SStructVarDecl& operator=(const SStructVarDecl& other) = delete;
    SYNTAX_API SStructVarDecl& operator=(SStructVarDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(this); }

};

class SEnumElemVarDecl
    : virtual public SSyntax
{
public:
    STypeExp* type;
    std::string memberName;

    SYNTAX_API SEnumElemVarDecl(STypeExp* type, std::string memberName);
    SEnumElemVarDecl(const SEnumElemVarDecl&) = delete;
    SYNTAX_API SEnumElemVarDecl(SEnumElemVarDecl&&) noexcept;
    SYNTAX_API ~SEnumElemVarDecl();

    SEnumElemVarDecl& operator=(const SEnumElemVarDecl& other) = delete;
    SYNTAX_API SEnumElemVarDecl& operator=(SEnumElemVarDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class SEnumElemDecl
    : virtual public SSyntax
{
public:
    std::string memberName;
    std::vector<SEnumElemVarDecl*> vars;

    SYNTAX_API SEnumElemDecl(std::string memberName, std::vector<SEnumElemVarDecl*> vars);
    SEnumElemDecl(const SEnumElemDecl&) = delete;
    SYNTAX_API SEnumElemDecl(SEnumElemDecl&&) noexcept;
    SYNTAX_API ~SEnumElemDecl();

    SEnumElemDecl& operator=(const SEnumElemDecl& other) = delete;
    SYNTAX_API SEnumElemDecl& operator=(SEnumElemDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};

class SEnumDecl
    : public SClassMemberDecl
    , public SStructMemberDecl
    , public SNamespaceDeclElement
    , public SScriptElement
{
public:
    std::optional<SAccessModifier> accessModifier;
    std::string memberName;
    std::vector<STypeParam> typeParams;
    std::vector<SEnumElemDecl*> elements;

    SYNTAX_API SEnumDecl(std::optional<SAccessModifier> accessModifier, std::string memberName, std::vector<STypeParam> typeParams, std::vector<SEnumElemDecl*> elements);
    SEnumDecl(const SEnumDecl&) = delete;
    SYNTAX_API SEnumDecl(SEnumDecl&&) noexcept;
    SYNTAX_API virtual ~SEnumDecl();

    SEnumDecl& operator=(const SEnumDecl& other) = delete;
    SYNTAX_API SEnumDecl& operator=(SEnumDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SClassMemberDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(SStructMemberDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(SNamespaceDeclElementVisitor& visitor) override { visitor.Visit(this); }
    void Accept(SScriptElementVisitor& visitor) override { visitor.Visit(this); }

};

class SNamespaceDecl
    : public SNamespaceDeclElement
    , public SScriptElement
{
public:
    std::vector<std::string> names;
    std::vector<SNamespaceDeclElement*> elements;

    SYNTAX_API SNamespaceDecl(std::vector<std::string> names, std::vector<SNamespaceDeclElement*> elements);
    SNamespaceDecl(const SNamespaceDecl&) = delete;
    SYNTAX_API SNamespaceDecl(SNamespaceDecl&&) noexcept;
    SYNTAX_API virtual ~SNamespaceDecl();

    SNamespaceDecl& operator=(const SNamespaceDecl& other) = delete;
    SYNTAX_API SNamespaceDecl& operator=(SNamespaceDecl&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
    void Accept(SNamespaceDeclElementVisitor& visitor) override { visitor.Visit(this); }
    void Accept(SScriptElementVisitor& visitor) override { visitor.Visit(this); }

};

class SScript
    : virtual public SSyntax
{
public:
    std::vector<SScriptElement*> elements;

    SYNTAX_API SScript(std::vector<SScriptElement*> elements);
    SScript(const SScript&) = delete;
    SYNTAX_API SScript(SScript&&) noexcept;
    SYNTAX_API ~SScript();

    SScript& operator=(const SScript& other) = delete;
    SYNTAX_API SScript& operator=(SScript&& other) noexcept;

    SYNTAX_API JsonItem ToJson();
};


}
