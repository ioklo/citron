#include "NStmt.h"

#include "NExp.h"

using namespace std;

namespace Citron {


NStmt_Command::NStmt_Command(vector<shared_ptr<NExp_String>>&& commands)
    : commands(std::move(commands))
{
}

NStmt_LocalVarDecl::NStmt_LocalVarDecl(const RTypePtr& type, const string& name, NExpPtr&& initExp)
    : type(type), name(name), initExp(std::move(initExp))
{

}

NStmt_If::NStmt_If(NExpPtr&& cond, vector<NStmtPtr>&& body, vector<NStmtPtr>&& elseBody)
    : cond(std::move(cond)), body(std::move(body)), elseBody(std::move(elseBody))
{

}

NStmt_IfNullableRefTest::NStmt_IfNullableRefTest(RTypePtr&& refType, RName&& varName, NExpPtr&& asExp, vector<NStmtPtr>&& body, vector<NStmtPtr>&& elseBody)
    : refType(std::move(refType)), varName(std::move(varName)), asExp(std::move(asExp)), body(std::move(body)), elseBody(std::move(elseBody))
{

}

NStmt_IfNullableValueTest::NStmt_IfNullableValueTest(RTypePtr&& type, RName&& varName, NExpPtr&& asExp, vector<NStmtPtr>&& body, vector<NStmtPtr>&& elseBody)
    : type(std::move(type)), varName(std::move(varName)), asExp(std::move(asExp)), body(std::move(body)), elseBody(std::move(elseBody))
{

}

NStmt_For::NStmt_For(vector<NStmtPtr>&& initStmts, NExpPtr&& condExp, NExpPtr&& continueExp, vector<NStmtPtr>&& body)
    : initStmts(std::move(initStmts)), condExp(std::move(condExp)), continueExp(std::move(continueExp)), body(std::move(body))
{

}

NStmt_Continue::NStmt_Continue()
{

}

NStmt_Break::NStmt_Break()
{

}

NStmt_Return::NStmt_Return(NExpPtr&& exp)
    : exp(std::move(exp))
{

}

NStmt_Block::NStmt_Block(vector<NStmtPtr>&& stmts)
    : stmts(std::move(stmts))
{

}

NStmt_Blank::NStmt_Blank()
{

}

NStmt_Exp::NStmt_Exp(NExpPtr&& exp)
    : exp(exp)
{

}

NStmt_Task::NStmt_Task(shared_ptr<NLambdaDecl>&& lambdaDecl, vector<NArgument>&& captureArgs)
    : lambdaDecl(std::move(lambdaDecl)), captureArgs(std::move(captureArgs))
{
}

NStmt_Await::NStmt_Await(vector<NStmtPtr>&& body)
    : body(std::move(body))
{

}

NStmt_Async::NStmt_Async(shared_ptr<NLambdaDecl>&& lambdaDecl, vector<NArgument>&& captureArgs)
    : lambdaDecl(std::move(lambdaDecl)), captureArgs(std::move(captureArgs))
{

}

NStmt_Foreach::NStmt_Foreach(NExpPtr&& enumeratorExp, RTypePtr&& itemType, const RName& varName, NExpPtr&& nextExp, vector<NStmtPtr>&& body)
    : enumeratorExp(std::move(enumeratorExp)), itemType(std::move(itemType)), varName(varName), nextExp(std::move(nextExp)), body(std::move(body))
{

}

NStmt_ForeachCast::NStmt_ForeachCast(NExpPtr&& enumeratorExp, RTypePtr&& itemType, const RName& varName, RTypePtr&& rawItemType, NExpPtr&& nextExp, NExpPtr&& castExp, vector<NStmtPtr>&& body)
    : enumeratorExp(std::move(enumeratorExp)), itemType(std::move(itemType)), varName(varName), nextExp(std::move(nextExp)), castExp(std::move(castExp)), body(std::move(body))
{

}

NStmt_Yield::NStmt_Yield(NExpPtr&& value)
    : value(std::move(value))
{

}

NStmt_CallClassCtor::NStmt_CallClassCtor()
{

}

NStmt_CallStructCtor::NStmt_CallStructCtor()
{

}

NStmt_NullDirective::NStmt_NullDirective(NLocPtr&& loc)
    : loc(std::move(loc))
{
}

NStmt_NotNullDirective::NStmt_NotNullDirective(NLocPtr&& loc)
    : loc(std::move(loc))
{

}

NStmt_StaticNullDirective::NStmt_StaticNullDirective()
{

}

NStmt_StaticNotNullDirective::NStmt_StaticNotNullDirective()
{

}

NStmt_StaticUnknownNullDirective::NStmt_StaticUnknownNullDirective()
{

}

}