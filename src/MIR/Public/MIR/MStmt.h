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
#include "MScopeKind.h"

namespace Citron {

class RType;
class RTypeArguments;

class RClassCtorDecl;
class RStructCtorDecl;

class NLambdaDecl;
class NStructCtorDecl;

struct MLoc;

struct MStmtVisitor;

enum class MStmt_AssignKind
{
    Copy,
    Move
};

// MTopLevel 계열
struct MTopLevel_Read { MRead read; };
struct MTopLevel_Create { MCreate create; };
struct MTopLevel_Loc { MLoc* loc; };
struct MTopLevel_Command { std::vector<MRead_Loc> commands; }; 
struct MTopLevel_Assign { MStmt_AssignKind kind;  MLoc* dest; MRead_Loc src; };
// o_catch는 try F() catch_* { }이 붙었을 경우, try F()는 에러가 compatible할때 try F() catch_error(error) { error } 로 변환된다
struct MTopLevel_Call { MCallable callable; std::vector<MArgument> args; std::optional<MCatch> o_catch; }; 

struct MStmt
{
    virtual ~MStmt() {}
    virtual void Accept(MStmtVisitor& visitor) = 0;
};

struct MStmt_Scope : MStmt
{
    MScopeKind scopeKind;
    std::vector<MStmt*> stmts;

    MStmt_Scope(MScopeKind&& scopeKind, std::vector<MStmt*>&& stmts)
        : scopeKind{std::move(scopeKind)}, stmts{std::move(stmts)}
    {
    }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_Command : MStmt
{
    MTopLevel_Command command;    

public:
    MStmt_Command(MTopLevel_Command&& command)
        : command{std::move(command)}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_LocalVarDeclInit_Uninit {};
struct MStmt_LocalVarDeclInit_Create { MTopLevel_Create create; };
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
    MTopLevel_Loc loc;

public:
    MStmt_LocalRefDecl(RType* type, const RName& name, MTopLevel_Loc&& loc)
        : type{type}, name{name}, loc{std::move(loc)}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

// 
struct MStmt_If : MStmt
{
    MTopLevel_Read cond; // BC

    MStmt_Scope* trueBody;
    MStmt_Scope* falseBody;

    MStmt_If(MTopLevel_Read&& cond, MStmt_Scope* trueBody, MStmt_Scope* falseBody)
        : cond{std::move(cond)}, trueBody{trueBody}, falseBody{falseBody}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_For : MStmt
{   
    std::optional<MTopLevel_Read> cond; // BC
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
    size_t labelId;

    MStmt_Continue(size_t labelId)
        : labelId{labelId}
    {
    }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_Break : MStmt
{
    size_t labelId;

    MStmt_Break(size_t labelId)
        : labelId{labelId}
    {
    }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_Return : MStmt
{
    std::optional<MTopLevel_Create> create;

public:
    MStmt_Return(std::optional<MTopLevel_Create>&& create)
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
    MTopLevel_Create create;

public:
    MStmt_Exp(MTopLevel_Create&& create)
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
    MTopLevel_Create iterCreate;

    // talias
    RType* itemType;
    RName itemName;
    // std::vector<MStmt*> nextItemStmts; // GetNext호출하고, null이면 종료, value면 itemName에 바인딩, outer scope에 넣는다

    MStmt_Scope* body;

public:
    MStmt_Foreach(RType* iterType, const RName& iterName, MTopLevel_Create&& iterCreate, RType* itemType, const RName& itemName, MStmt_Scope* body)
        : iterType{iterType}, iterName{iterName}, iterCreate{std::move(iterCreate)}, itemType{itemType}, itemName{itemName}, body{body}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_Yield : MStmt
{
    MTopLevel_Create valueCreate;

public:
    MStmt_Yield(MTopLevel_Create&& valueCreate)
        : valueCreate{std::move(valueCreate)}
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
    MTopLevel_Call call;
    
    MStmt_Call(MTopLevel_Call&& call)
        : call{std::move(call)}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

// NBC assign, void return assignment
struct MStmt_Assign : MStmt
{
    MTopLevel_Assign assign;

    MStmt_Assign(MTopLevel_Assign&& assign)
        : assign{std::move(assign)}
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