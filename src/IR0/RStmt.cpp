#include "RStmt.h"

#include "RExp.h"

using namespace std;

namespace Citron {


RStmt_Command::RStmt_Command(vector<shared_ptr<RExp_String>>&& commands)
    : commands(std::move(commands))
{
}

RStmt_LocalVarDecl::RStmt_LocalVarDecl(const RTypePtr& type, const string& name, RExpPtr&& initExp)
    : type(type), name(name), initExp(std::move(initExp))
{

}

RStmt_If::RStmt_If(RExpPtr&& cond, vector<RStmtPtr>&& body, vector<RStmtPtr>&& elseBody)
    : cond(std::move(cond)), body(std::move(body)), elseBody(std::move(elseBody))
{

}

RStmt_IfNullableRefTest::RStmt_IfNullableRefTest(RTypePtr&& refType, RName&& varName, RExpPtr&& asExp, vector<RStmtPtr>&& body, vector<RStmtPtr>&& elseBody)
    : refType(std::move(refType)), varName(std::move(varName)), asExp(std::move(asExp)), body(std::move(body)), elseBody(std::move(elseBody))
{

}

RStmt_IfNullableValueTest::RStmt_IfNullableValueTest(RTypePtr&& type, RName&& varName, RExpPtr&& asExp, vector<RStmtPtr>&& body, vector<RStmtPtr>&& elseBody)
    : type(std::move(type)), varName(std::move(varName)), asExp(std::move(asExp)), body(std::move(body)), elseBody(std::move(elseBody))
{

}

RStmt_For::RStmt_For(vector<RStmtPtr>&& initStmts, RExpPtr&& condExp, RExpPtr&& continueExp, vector<RStmtPtr>&& body)
    : initStmts(std::move(initStmts)), condExp(std::move(condExp)), continueExp(std::move(continueExp)), body(std::move(body))
{

}

RStmt_Continue::RStmt_Continue()
{

}

RStmt_Break::RStmt_Break()
{

}

RStmt_Return::RStmt_Return(RExpPtr&& exp)
    : exp(std::move(exp))
{

}

RStmt_Block::RStmt_Block(vector<RStmtPtr>&& stmts)
    : stmts(std::move(stmts))
{

}

RStmt_Blank::RStmt_Blank()
{

}

RStmt_Exp::RStmt_Exp(RExpPtr&& exp)
    : exp(exp)
{

}

RStmt_Task::RStmt_Task(shared_ptr<RLambdaDecl>&& lambdaDecl, vector<RArgument>&& captureArgs)
    : lambdaDecl(std::move(lambdaDecl)), captureArgs(std::move(captureArgs))
{
}

RStmt_Await::RStmt_Await(vector<RStmtPtr>&& body)
    : body(std::move(body))
{

}

RStmt_Async::RStmt_Async(shared_ptr<RLambdaDecl>&& lambdaDecl, vector<RArgument>&& captureArgs)
    : lambdaDecl(std::move(lambdaDecl)), captureArgs(std::move(captureArgs))
{

}

RStmt_Foreach::RStmt_Foreach(RExpPtr&& enumeratorExp, RTypePtr&& itemType, const RName& varName, RExpPtr&& nextExp, vector<RStmtPtr>&& body)
    : enumeratorExp(std::move(enumeratorExp)), itemType(std::move(itemType)), varName(varName), nextExp(std::move(nextExp)), body(std::move(body))
{

}

RStmt_ForeachCast::RStmt_ForeachCast(RExpPtr&& enumeratorExp, RTypePtr&& itemType, const RName& varName, RTypePtr&& rawItemType, RExpPtr&& nextExp, RExpPtr&& castExp, vector<RStmtPtr>&& body)
    : enumeratorExp(std::move(enumeratorExp)), itemType(std::move(itemType)), varName(varName), nextExp(std::move(nextExp)), castExp(std::move(castExp)), body(std::move(body))
{

}

RStmt_Yield::RStmt_Yield(RExpPtr&& value)
    : value(std::move(value))
{

}

RStmt_CallClassConstructor::RStmt_CallClassConstructor()
{

}

RStmt_CallStructConstructor::RStmt_CallStructConstructor()
{

}

RStmt_NullDirective::RStmt_NullDirective(RLocPtr&& loc)
    : loc(std::move(loc))
{
}

RStmt_NotNullDirective::RStmt_NotNullDirective(RLocPtr&& loc)
    : loc(std::move(loc))
{

}

RStmt_StaticNullDirective::RStmt_StaticNullDirective()
{

}

RStmt_StaticNotNullDirective::RStmt_StaticNotNullDirective()
{

}

RStmt_StaticUnknownNullDirective::RStmt_StaticUnknownNullDirective()
{

}

}