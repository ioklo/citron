#pragma once
#include "SyntaxConfig.h"
#include <optional>
#include <vector>
#include <string>
#include <variant>
#include <Infra/Json.h>

#include "ExpSyntaxes.h"
#include "VarDeclSyntax.h"
#include "EmbeddableStmtSyntaxes.h"
#include "ForStmtInitializerSyntaxes.h"

#include "SyntaxMacros.h"

namespace Citron {

using StmtSyntax = std::variant <
    class CommandStmtSyntax,
    class VarDeclStmtSyntax,
    class IfStmtSyntax,
    class IfTestStmtSyntax,
    class ForStmtSyntax,

    class ContinueStmtSyntax,
    class BreakStmtSyntax,
    class ReturnStmtSyntax,
    class BlockStmtSyntax,
    class BlankStmtSyntax,
    class ExpStmtSyntax,

    class TaskStmtSyntax,
    class AwaitStmtSyntax,
    class AsyncStmtSyntax,
    class ForeachStmtSyntax,
    class YieldStmtSyntax,

    class DirectiveStmtSyntax
>;


// 명령어
// TODO: commands의 Length가 1인 contract를 추가하자
class CommandStmtSyntax
{
    std::vector<StringExpSyntax> commands;

public:
    SYNTAX_API CommandStmtSyntax(std::vector<StringExpSyntax> commands);
    DECLARE_DEFAULTS(CommandStmtSyntax)

    std::vector<StringExpSyntax>& GetCommands() { return commands; }

    SYNTAX_API JsonItem ToJson();
};

// int a = 0, b, c;
class VarDeclStmtSyntax
{
    VarDeclSyntax varDecl;

public:
    VarDeclStmtSyntax(VarDeclSyntax varDecl) : varDecl(std::move(varDecl)) { }
    VarDeclSyntax& GetVarDecl() { return varDecl; }

    SYNTAX_API JsonItem ToJson();
};

// if ($cond) $body else $ElseBody
// recursive { (SingleEmbeddableStmt)body.stmt }
class IfStmtSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API IfStmtSyntax(ExpSyntax cond, EmbeddableStmtSyntax body, std::optional<EmbeddableStmtSyntax> elseBody);
    DECLARE_DEFAULTS(IfStmtSyntax)

    SYNTAX_API ExpSyntax& GetCond();
    SYNTAX_API EmbeddableStmtSyntax& GetBody();
    SYNTAX_API std::optional<EmbeddableStmtSyntax>& GetElseBody();

    SYNTAX_API JsonItem ToJson();
};

// if (testType varName = exp
// recursive { (SingleEmbeddableStmt)body.stmt }
class IfTestStmtSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API IfTestStmtSyntax(TypeExpSyntax testTypeExp, std::u32string varName, ExpSyntax exp, EmbeddableStmtSyntax body, std::optional<EmbeddableStmtSyntax> elseBody);
    DECLARE_DEFAULTS(IfTestStmtSyntax)

    SYNTAX_API TypeExpSyntax& GetTestTypeExp();
    SYNTAX_API std::u32string& GetVarName();
    SYNTAX_API ExpSyntax& GetExp();
    SYNTAX_API EmbeddableStmtSyntax& GetBody();
    SYNTAX_API std::optional<EmbeddableStmtSyntax>& GetElseBody();

    SYNTAX_API JsonItem ToJson();
};

// recursive { (SingleEmbeddableStmt)body.stmt }
class ForStmtSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API ForStmtSyntax(std::optional<ForStmtInitializerSyntax> initializer, std::optional<ExpSyntax> condExp, std::optional<ExpSyntax> continueExp, EmbeddableStmtSyntax body);
    DECLARE_DEFAULTS(ForStmtSyntax)

    SYNTAX_API std::optional<ForStmtInitializerSyntax>& GetInitializer();
    SYNTAX_API std::optional<ExpSyntax>& GetCondExp();
    SYNTAX_API std::optional<ExpSyntax>& GetContinueExp();
    SYNTAX_API EmbeddableStmtSyntax& GetBody();

    SYNTAX_API JsonItem ToJson();
};

class ContinueStmtSyntax
{
public:
    SYNTAX_API JsonItem ToJson();
};

class BreakStmtSyntax
{
public:
    SYNTAX_API JsonItem ToJson();
};

struct ReturnValueSyntaxInfo
{
    ExpSyntax value;
};

SYNTAX_API JsonItem ToJson(ReturnValueSyntaxInfo&);

class ReturnStmtSyntax
{
    std::optional<ReturnValueSyntaxInfo> info;

public:
    SYNTAX_API ReturnStmtSyntax(std::optional<ReturnValueSyntaxInfo> info);
    DECLARE_DEFAULTS(ReturnStmtSyntax)

    std::optional<ReturnValueSyntaxInfo>& GetInfo() { return info; }
    SYNTAX_API JsonItem ToJson();
};

// Stmt중간에 명시적으로 { }를 사용한 경우에만 BlockStmt로 나타낸다. 함수, for, async, await, if등에 나타나는 { 이후 구문들은 EmbeddableStmt를 사용한다
class BlockStmtSyntax
{
    std::vector<StmtSyntax> stmts;

public:
    SYNTAX_API BlockStmtSyntax(std::vector<StmtSyntax> stmts);
    DECLARE_DEFAULTS(BlockStmtSyntax)

    std::vector<StmtSyntax>& GetStmts() { return stmts; }
    SYNTAX_API JsonItem ToJson();
};

class BlankStmtSyntax
{
public:
    SYNTAX_API JsonItem ToJson();
};

class ExpStmtSyntax
{
    ExpSyntax exp;

public:
    ExpStmtSyntax(ExpSyntax exp)
        : exp(exp) { }

    ExpSyntax& GetExp() { return exp; }
    SYNTAX_API JsonItem ToJson();
};

class TaskStmtSyntax // task { ... }
{
    std::vector<StmtSyntax> body;

public:
    SYNTAX_API TaskStmtSyntax(std::vector<StmtSyntax> body);
    DECLARE_DEFAULTS(TaskStmtSyntax)

    std::vector<StmtSyntax>& GetStmts() { return body; }
    SYNTAX_API JsonItem ToJson();
};

class AwaitStmtSyntax // await { ... }
{
    std::vector<StmtSyntax> body;

public:
    SYNTAX_API AwaitStmtSyntax(std::vector<StmtSyntax> body);
    DECLARE_DEFAULTS(AwaitStmtSyntax)

    std::vector<StmtSyntax>& GetStmts() { return body; }
    SYNTAX_API JsonItem ToJson();
};

class AsyncStmtSyntax // async { ... }
{
    std::vector<StmtSyntax> body;

public:
    SYNTAX_API AsyncStmtSyntax(std::vector<StmtSyntax> body);
    DECLARE_DEFAULTS(AsyncStmtSyntax)

    std::vector<StmtSyntax>& GetStmts() { return body; }
    SYNTAX_API JsonItem ToJson();
};

// recursive { (SingleEmbeddableStmt)body.stmt }
class ForeachStmtSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API ForeachStmtSyntax(TypeExpSyntax type, std::u32string varName, ExpSyntax enumerable, EmbeddableStmtSyntax body);
    DECLARE_DEFAULTS(ForeachStmtSyntax)

    SYNTAX_API TypeExpSyntax& GetType();
    SYNTAX_API std::u32string& GetVarName();
    SYNTAX_API ExpSyntax& GetEnumerable();
    SYNTAX_API EmbeddableStmtSyntax& GetBody();

    SYNTAX_API JsonItem ToJson();
};

class YieldStmtSyntax
{
    ExpSyntax value;

public:
    YieldStmtSyntax(ExpSyntax value)
        : value(value) { }

    ExpSyntax& GetValue() { return value; }

    SYNTAX_API JsonItem ToJson();
};

// Stmt에 사용되는 Directive랑 Decl-Level에서 사용되는 Directive가 다르므로 구분해도 될 것 같다
class DirectiveStmtSyntax
{
    std::u32string name;
    std::vector<ExpSyntax> args;

public:
    DirectiveStmtSyntax(std::u32string name, std::vector<ExpSyntax> args)
        : name(std::move(name)), args(std::move(args)) { }

    std::u32string& GetName() { return name; }
    std::vector<ExpSyntax>& GetArgs() { return args; }

    SYNTAX_API JsonItem ToJson();
};

SYNTAX_API JsonItem ToJson(StmtSyntax& syntax);

}

