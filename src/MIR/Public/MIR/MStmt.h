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
class RStructCtorDecl;

class MExp_String;
class NLambdaDecl;
class NStructCtorDecl;

struct MLoc;

struct MStmtVisitor;
struct MStmt
{
    virtual ~MStmt() {}
    virtual void Accept(MStmtVisitor& visitor) = 0;
};

struct MStmt_Command : MStmt
{
    std::vector<MExp_String*> commands;

public:
    MStmt_Command(std::vector<MExp_String*>&& commands)
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
    MStmt_LocalRefDecl(RType* type, RName&& name, MLoc* loc)
        : type{type}, name{std::move(name)}, loc{loc}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_If : MStmt
{
    MExp* cond; // BC
    std::vector<MStmt*> body;
    std::vector<MStmt*> elseBody;

public:
    MStmt_If(MExp* cond, std::vector<MStmt*>&& body, std::vector<MStmt*>&& elseBody)
        : cond{cond}, body{std::move(body)}, elseBody{std::move(elseBody)}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

// if (exp is not_null(c)) 
// IfTest_NullableInplace(exp, C, c, body, elseBody)
// 
// if (s is not_null(s))
// IfTest_Nullable(exp, 
// 
// if (c is D(d)) { }

// not_null(s)
struct MPattern_NotNull { RName name; }; // talias

// null
struct MPattern_Null { };

// C(c)
struct MPattern_RefType { RType* type; RName name; };

using MPattern = std::variant<struct MPattern_NotNull, struct MPattern_Null, MPattern_RefType>;

// if (target match pattern) { }
struct MStmt_IfTest : MStmt
{
    MRead target;
    MPattern* pattern;
    std::vector<MStmt*> body;
    std::vector<MStmt*> elseBody;
};

struct MStmt_IfNullableRefTest : MStmt
{
    RType* refType;
    RName varName;
    MExp* asExp;
    std::vector<MStmt*> body;
    std::vector<MStmt*> elseBody;

public:
    MStmt_IfNullableRefTest(RType* refType, RName&& varName, MExp* asExp, std::vector<MStmt*>&& body, std::vector<MStmt*>&& elseBody)
        : refType{refType}, varName{std::move(varName)}, asExp{asExp}, body{std::move(body)}, elseBody{std::move(elseBody)}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_IfNullableValueTest : MStmt
{
    RType* type;
    RName varName;
    MExp* asExp;
    std::vector<MStmt*> body;
    std::vector<MStmt*> elseBody;

public:
    MStmt_IfNullableValueTest(RType* type, RName&& varName, MExp* asExp, std::vector<MStmt*>&& body, std::vector<MStmt*>&& elseBody)
        : type{type}, varName{std::move(varName)}, asExp{asExp}, body{std::move(body)}, elseBody{std::move(elseBody)}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_For : MStmt
{
    std::vector<MStmt*> initStmts;
    MExp* condExp;
    MExp* continueExp;
    std::vector<MStmt*> body;

public:
    MStmt_For(std::vector<MStmt*>&& initStmts, MExp* condExp, MExp* continueExp, std::vector<MStmt*>&& body)
        : initStmts{std::move(initStmts)}, condExp{condExp}, continueExp{continueExp}, body{std::move(body)}
    { }
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
    MExp* exp;

public:
    MStmt_Return(MExp* exp)
        : exp{exp}
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_Block : MStmt
{
    std::vector<MStmt*> stmts;

public:
    MStmt_Block(std::vector<MStmt*>&& stmts)
        : stmts(std::move(stmts))
    { }
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_Blank : MStmt
{
    MIR_API void Accept(MStmtVisitor& visitor) override;
};

struct MStmt_Exp : MStmt
{
    MExp* exp;

public:
    MStmt_Exp(MExp* exp)
        : exp{exp}
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
    std::vector<MStmt*> body;

public:
    MStmt_Await(std::vector<MStmt*>&& body)
        : body{std::move(body)} 
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
    std::vector<MStmt*> nextItemStmts; // GetNext호출하고, null이면 종료, value면 itemName에 바인딩

    std::vector<MStmt*> body;

public:
    MStmt_Foreach(RType* iterType, const RName& iterName, MCreate iterCreate, RType* itemType, const RName& itemName, std::vector<MStmt*>&& nextItemStmts)
        : iterType{iterType}, iterName{iterName}, iterCreate{iterCreate}, itemType{itemType}, itemName{itemName}, nextItemStmts{std::move(nextItemStmts)}
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

struct MDirective_NullDirective { MLoc* loc; };
struct MDirective_NotNullDirective { MLoc* loc; };
struct MDirective_StaticNullDirective { MLoc* loc; };
struct MDirective_StaticNotNullDirective { MLoc* loc; };
struct MDirective_StaticUnknownDirective { MLoc* loc; };

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

}

#include "MStmtVisitor.g.h"

