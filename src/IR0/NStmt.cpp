module Citron.NDecls:NStmt;

import :NExp;

using namespace std;

namespace Citron {


NStmt_Command::NStmt_Command(vector<shared_ptr<NExp_String>>&& commands)
    : commands(move(commands))
{
}

NStmt_LocalVarDecl::NStmt_LocalVarDecl(const RTypePtr& type, const string& name, NExpPtr&& initExp)
    : type(type), name(name), initExp(move(initExp))
{

}

NStmt_If::NStmt_If(NExpPtr&& cond, vector<NStmtPtr>&& body, vector<NStmtPtr>&& elseBody)
    : cond(move(cond)), body(move(body)), elseBody(move(elseBody))
{

}

NStmt_IfNullableRefTest::NStmt_IfNullableRefTest(RTypePtr&& refType, RName&& varName, NExpPtr&& asExp, vector<NStmtPtr>&& body, vector<NStmtPtr>&& elseBody)
    : refType(move(refType)), varName(move(varName)), asExp(move(asExp)), body(move(body)), elseBody(move(elseBody))
{

}

NStmt_IfNullableValueTest::NStmt_IfNullableValueTest(RTypePtr&& type, RName&& varName, NExpPtr&& asExp, vector<NStmtPtr>&& body, vector<NStmtPtr>&& elseBody)
    : type(move(type)), varName(move(varName)), asExp(move(asExp)), body(move(body)), elseBody(move(elseBody))
{

}

NStmt_For::NStmt_For(vector<NStmtPtr>&& initStmts, NExpPtr&& condExp, NExpPtr&& continueExp, vector<NStmtPtr>&& body)
    : initStmts(move(initStmts)), condExp(move(condExp)), continueExp(move(continueExp)), body(move(body))
{

}

NStmt_Continue::NStmt_Continue()
{

}

NStmt_Break::NStmt_Break()
{

}

NStmt_Return::NStmt_Return(NExpPtr&& exp)
    : exp(move(exp))
{

}

NStmt_Block::NStmt_Block(vector<NStmtPtr>&& stmts)
    : stmts(move(stmts))
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
    : lambdaDecl(move(lambdaDecl)), captureArgs(move(captureArgs))
{
}

NStmt_Await::NStmt_Await(vector<NStmtPtr>&& body)
    : body(move(body))
{

}

NStmt_Async::NStmt_Async(shared_ptr<NLambdaDecl>&& lambdaDecl, vector<NArgument>&& captureArgs)
    : lambdaDecl(move(lambdaDecl)), captureArgs(move(captureArgs))
{

}

NStmt_Foreach::NStmt_Foreach(NExpPtr&& enumeratorExp, RTypePtr&& itemType, const RName& varName, NExpPtr&& nextExp, vector<NStmtPtr>&& body)
    : enumeratorExp(move(enumeratorExp)), itemType(move(itemType)), varName(varName), nextExp(move(nextExp)), body(move(body))
{

}

NStmt_ForeachCast::NStmt_ForeachCast(NExpPtr&& enumeratorExp, RTypePtr&& itemType, const RName& varName, RTypePtr&& rawItemType, NExpPtr&& nextExp, NExpPtr&& castExp, vector<NStmtPtr>&& body)
    : enumeratorExp(move(enumeratorExp)), itemType(move(itemType)), varName(varName), nextExp(move(nextExp)), castExp(move(castExp)), body(move(body))
{

}

NStmt_Yield::NStmt_Yield(NExpPtr&& value)
    : value(move(value))
{

}

NStmt_CallClassCtor::NStmt_CallClassCtor()
{

}

NStmt_CallStructCtor::NStmt_CallStructCtor()
{

}

NStmt_NullDirective::NStmt_NullDirective(NLocPtr&& loc)
    : loc(move(loc))
{
}

NStmt_NotNullDirective::NStmt_NotNullDirective(NLocPtr&& loc)
    : loc(move(loc))
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