#pragma once

#include "MIRConfig.h"

#include <variant>
#include <vector>
#include <string>
#include <optional>

#include "RSymbol/RNames.h"
#include "MArgument.h"
#include "MCallable.h"
#include "MCatch.h"
#include "MRead.h"

namespace Citron {

class RType;
class RTypeArguments;

class RClassCtorDecl;
class RStructCtorDecl;

class NLambdaDecl;
class NStructCtorDecl;

struct MLoc;

struct MStmtVisitor;

struct MStmt
{
    virtual ~MStmt() {}
    virtual void Accept(MStmtVisitor& visitor) = 0;
};

struct MStmt_Scope : MStmt
{
    std::vector<MStmt*> stmts;

public:
    MStmt_Scope(std::vector<MStmt*>&& stmts)
        : stmts{std::move(stmts)}
    {
    }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_Command : MStmt
{
    std::vector<MRead_Loc> commands;

public:
    MStmt_Command(std::vector<MRead_Loc>&& commands)
        : commands{std::move(commands)}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_LocalVarDeclInit_Uninit {};
struct MStmt_LocalVarDeclInit_Create { MCreate create; };
using MStmt_LocalVarDeclInit = std::variant<MStmt_LocalVarDeclInit_Uninit, MStmt_LocalVarDeclInit_Create>;

struct MStmt_LocalVarDecl : MStmt
{
    RType* type;
    RName name;
    MStmt_LocalVarDeclInit init;

public:
    MStmt_LocalVarDecl(RType* type, const RName& name, MStmt_LocalVarDeclInit init)
        : type{type}, name{name}, init{init}
    {}

    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_LocalRefDecl : MStmt
{
    RType* type;
    RName name;
    MLoc* loc;

public:
    MStmt_LocalRefDecl(RType* type, const RName& name, MLoc* loc)
        : type{type}, name{name}, loc{loc}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

// 
struct MStmt_If : MStmt
{
    MRead cond; // BC

    MStmt_Scope* trueBody;
    MStmt_Scope* falseBody;

    MStmt_If(MRead&& cond, MStmt_Scope* trueBody, MStmt_Scope* falseBody)
        : cond{std::move(cond)}, trueBody{trueBody}, falseBody{falseBody}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_For : MStmt
{   
    std::optional<MRead> cond; // BC
    MStmt* contStmt;
    MStmt_Scope* body;

public:
    MStmt_For(std::optional<MRead>&& cond, MStmt* contStmt, MStmt_Scope* body)
        : cond{std::move(cond)}, contStmt{contStmt}, body{body}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_While : MStmt
{
    std::optional<MRead> cond; // BC
    MStmt_Scope* body;

    MStmt_While(std::optional<MRead>&& cond, MStmt_Scope* body)
        : cond{std::move(cond)}, body{body}
    {
    }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_Continue : MStmt
{
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_Break : MStmt
{
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_Return : MStmt
{
    std::optional<MCreate> create;

public:
    MStmt_Return(std::optional<MCreate>&& create)
        : create{std::move(create)}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_Blank : MStmt
{
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

// 이름은 Exp지만, Exp, InitExp둘다 받는다
struct MStmt_Exp : MStmt
{
    MCreate create;

public:
    MStmt_Exp(MCreate&& create)
        : create{std::move(create)}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_Task : MStmt
{
    NLambdaDecl* lambdaDecl;
    std::vector<MArgument> captureArgs;

public:
    MStmt_Task(NLambdaDecl* lambdaDecl, std::vector<MArgument>&& captureArgs)
        : lambdaDecl{lambdaDecl}, captureArgs{std::move(captureArgs)}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_Await : MStmt
{
    MStmt_Scope* body;

public:
    MStmt_Await(MStmt_Scope* body)
        : body{body}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_Async : MStmt
{
    NLambdaDecl* lambdaDecl;
    std::vector<MArgument> captureArgs;

public:
    MStmt_Async(NLambdaDecl* lambdaDecl, std::vector<MArgument>&& captureArgs)
        : lambdaDecl{lambdaDecl}, captureArgs{std::move(captureArgs)}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

// SYNTAX: foreach(TItem& item : list)
// foreach(TListIter, iterName, iter = GetIterator(list), TItem, item, nextExp = GetNext) { body }
struct MStmt_Foreach : MStmt
{
    RType* iterType;
    RName iterName;
    MCreate iterCreate;

    // talias
    RType* itemType;
    RName itemName;
    // std::vector<MStmt*> nextItemStmts; // GetNext호출하고, null이면 종료, value면 itemName에 바인딩, outer scope에 넣는다

    MStmt_Scope* body;

public:
    MStmt_Foreach(RType* iterType, const RName& iterName, MCreate iterCreate, RType* itemType, const RName& itemName, MStmt_Scope* body)
        : iterType{iterType}, iterName{iterName}, iterCreate{iterCreate}, itemType{itemType}, itemName{itemName}, body{body}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_Yield : MStmt
{
    MCreate valueCreate;

public:
    MStmt_Yield(MCreate&& valueCreate)
        : valueCreate{valueCreate}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

// Ctor 내에서 상위 Ctor 호출시 사용
struct MStmt_CallBaseClassCtor : MStmt
{
    RClassCtorDecl* ctor;
    std::vector<MArgument> args;

public:
    MStmt_CallBaseClassCtor(RClassCtorDecl* ctor, std::vector<MArgument>&& args)
        : ctor{ctor}, args{std::move(args)}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_CallBaseStructCtor : MStmt
{
    NStructCtorDecl* ctor;
    RTypeArguments* typeArgs;
    std::vector<MArgument> args;

public:
    MStmt_CallBaseStructCtor(NStructCtorDecl* ctor, RTypeArguments* typeArgs, std::vector<MArgument>&& args)
        : ctor{ctor}, typeArgs{typeArgs}, args{std::move(args)}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MDirective_NullDirective { MRead loc; };
struct MDirective_NotNullDirective { MRead loc; };
struct MDirective_StaticNullDirective { MRead loc; };
struct MDirective_StaticNotNullDirective { MRead loc; };
struct MDirective_StaticUnknownDirective { MRead loc; };

using MDirective = std::variant<
    MDirective_NullDirective,
    MDirective_NotNullDirective,
    MDirective_StaticNullDirective,
    MDirective_StaticNotNullDirective,
    MDirective_StaticUnknownDirective>;

struct MStmt_Directive : MStmt
{
    MDirective directive;

public:
    MStmt_Directive(MDirective&& directive)
        : directive{std::move(directive)}
    { }

    MIR_API void Accept(MStmtVisitor& visitor) override;
};

// void return
struct MStmt_Call : MStmt
{
    MCallable callable;
    std::vector<MArgument> args;
    std::optional<MCatch> o_catch; // try F() catch_* { }이 붙었을 경우

    MStmt_Call(MCallable&& callable, std::vector<MArgument>&& args, std::optional<MCatch>&& o_catch)
        : callable{std::move(callable)}, args{std::move(args)}, o_catch{std::move(o_catch)}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

enum class MStmt_AssignKind
{
    Copy,
    Move
};

// NBC assign, void return assignment
struct MStmt_Assign : MStmt
{
    MStmt_AssignKind kind;
    MLoc* dest;
    MRead_Loc src; // NBC

    MStmt_Assign(MStmt_AssignKind kind, MLoc* dest, MRead_Loc&& src)
        : kind{kind}, dest{dest}, src{std::move(src)}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

// do { ... } catch_...
struct MStmt_Do : MStmt
{
    MStmt_Scope* body;
    std::vector<MCatch> catches;

    MIR_API void Accept(MStmtVisitor& visitor) override;
};

}

#include "MStmtVisitor.g.h"