#pragma once
#include "SyntaxConfig.h"
#include <optional>
#include <vector>
#include <string>
#include <variant>

#include "Exps.h"
#include "VarDecl.h"
#include "EmbeddableStmts.h"
#include "ForStmtInitializers.h"

#include "SyntaxMacros.h"

namespace Citron::Syntax {

using Stmt = std::variant <
    class CommandStmt,
    class VarDeclStmt,
    class IfStmt,
    class IfTestStmt,
    class ForStmt,

    class ContinueStmt,
    class BreakStmt,
    class ReturnStmt,
    class BlockStmt,
    class BlankStmt,
    class ExpStmt,

    class TaskStmt,
    class AwaitStmt,
    class AsyncStmt,
    class ForeachStmt,
    class YieldStmt,

    class DirectiveStmt
>;


// 명령어
// TODO: commands의 Length가 1인 contract를 추가하자
class CommandStmt
{
    std::vector<StringExp> commands;

public:
    SYNTAX_API CommandStmt(std::vector<StringExp> commands);
    DECLARE_DEFAULTS(CommandStmt)

    std::vector<StringExp>& GetCommands() { return commands; }
};

// int a = 0, b, c;
class VarDeclStmt
{
    VarDecl varDecl;

public:
    VarDeclStmt(VarDecl varDecl) : varDecl(std::move(varDecl)) { }
    VarDecl& GetVarDecl() { return varDecl; }
};

// if ($cond) $body else $ElseBody
// recursive { (SingleEmbeddableStmt)body.stmt }
class IfStmt
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API IfStmt(Exp cond, EmbeddableStmt body, std::optional<EmbeddableStmt> elseBody);
    DECLARE_DEFAULTS(IfStmt)

    SYNTAX_API Exp& GetCond();
    SYNTAX_API EmbeddableStmt& GetBody();
    SYNTAX_API std::optional<EmbeddableStmt>& GetElseBody();
};

// if (testType varName = exp
// recursive { (SingleEmbeddableStmt)body.stmt }
class IfTestStmt
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API IfTestStmt(TypeExp testTypeExp, std::string varName, Exp exp, EmbeddableStmt body, std::optional<EmbeddableStmt> elseBody);
    DECLARE_DEFAULTS(IfTestStmt)

    SYNTAX_API TypeExp& GetTestTypeExp();
    SYNTAX_API std::string& GetVarName();
    SYNTAX_API Exp& GetExp();
    SYNTAX_API EmbeddableStmt& GetBody();
    SYNTAX_API std::optional<EmbeddableStmt>& GetElseBody();
};

// recursive { (SingleEmbeddableStmt)body.stmt }
class ForStmt
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API ForStmt(std::optional<ForStmtInitializer> initializer, std::optional<Exp> condExp, std::optional<Exp> continueExp, EmbeddableStmt body);
    DECLARE_DEFAULTS(ForStmt)

    SYNTAX_API std::optional<ForStmtInitializer>& GetInitializer();
    SYNTAX_API std::optional<Exp>& GetCondExp();
    SYNTAX_API std::optional<Exp>& GetContinueExp();
    SYNTAX_API EmbeddableStmt& GetBody();
};

class ContinueStmt
{
};

class BreakStmt
{
};

struct ReturnValueInfo
{
    Exp value;
};

class ReturnStmt
{
    std::optional<ReturnValueInfo> info;

public:
    SYNTAX_API ReturnStmt(std::optional<ReturnValueInfo> info);
    DECLARE_DEFAULTS(ReturnStmt)

    std::optional<ReturnValueInfo>& GetInfo() { return info; }
};

// Stmt중간에 명시적으로 { }를 사용한 경우에만 BlockStmt로 나타낸다. 함수, for, async, await, if등에 나타나는 { 이후 구문들은 EmbeddableStmt를 사용한다
class BlockStmt
{
    std::vector<Stmt> stmts;

public:
    SYNTAX_API BlockStmt(std::vector<Stmt> stmts);
    DECLARE_DEFAULTS(BlockStmt)

    std::vector<Stmt>& GetStmts() { return stmts; }
};

class BlankStmt
{
};

class ExpStmt
{
    Exp exp;

public:
    ExpStmt(Exp exp)
        : exp(exp) { }

    Exp& GetExp() { return exp; }
};

class TaskStmt // task { ... }
{
    std::vector<Stmt> body;

public:
    SYNTAX_API TaskStmt(std::vector<Stmt> body);
    DECLARE_DEFAULTS(TaskStmt)

    std::vector<Stmt>& GetStmts() { return body; }
};

class AwaitStmt // await { ... }
{
    std::vector<Stmt> body;

public:
    SYNTAX_API AwaitStmt(std::vector<Stmt> body);
    DECLARE_DEFAULTS(AwaitStmt)

    std::vector<Stmt>& GetStmts() { return body; }
};

class AsyncStmt // async { ... }
{
    std::vector<Stmt> body;

public:
    SYNTAX_API AsyncStmt(std::vector<Stmt> body);
    DECLARE_DEFAULTS(AsyncStmt)

    std::vector<Stmt>& GetStmts() { return body; }
};

// recursive { (SingleEmbeddableStmt)body.stmt }
class ForeachStmt
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API ForeachStmt(TypeExp type, std::string varName, Exp enumerable, EmbeddableStmt body);
    DECLARE_DEFAULTS(ForeachStmt)

    SYNTAX_API TypeExp& GetType();
    SYNTAX_API std::string& GetVarName();
    SYNTAX_API Exp& GetEnumerable();
    SYNTAX_API EmbeddableStmt& GetBody();
};

class YieldStmt
{
    Exp value;

public:
    YieldStmt(Exp value)
        : value(value) { }

    Exp& GetValue() { return value; }
};

// Stmt에 사용되는 Directive랑 Decl-Level에서 사용되는 Directive가 다르므로 구분해도 될 것 같다
class DirectiveStmt
{
    std::string name;
    std::vector<Exp> args;

public:
    DirectiveStmt(std::string name, std::vector<Exp> args)
        : name(std::move(name)), args(std::move(args)) { }

    std::string& GetName() { return name; }
    std::vector<Exp>& GetArgs() { return args; }
};

}

