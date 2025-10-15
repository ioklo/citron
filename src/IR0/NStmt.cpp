#include "NStmt.h"

#include "NExp.h"

using namespace std;

namespace Citron {


NStmt_Command::NStmt_Command(vector<NExp_String*>&& commands)
    : commands(move(commands))
{
}

NStmt_LocalVarDecl::NStmt_LocalVarDecl(RType* type, const string& name, NExp* initExp)
    : type{type}, name{name}, initExp{initExp}
{

}

NStmt_If::NStmt_If(NExp* cond, vector<NStmt*>&& body, vector<NStmt*>&& elseBody)
    : cond{cond}, body{move(body)}, elseBody(move(elseBody))
{

}

NStmt_IfNullableRefTest::NStmt_IfNullableRefTest(RType* refType, RName&& varName, NExp* asExp, vector<NStmt*>&& body, vector<NStmt*>&& elseBody)
    : refType(refType), varName(move(varName)), asExp(asExp), body(move(body)), elseBody(move(elseBody))
{

}

NStmt_IfNullableValueTest::NStmt_IfNullableValueTest(RType* type, RName&& varName, NExp* asExp, vector<NStmt*>&& body, vector<NStmt*>&& elseBody)
    : type(type), varName(move(varName)), asExp(asExp), body(move(body)), elseBody(move(elseBody))
{

}

NStmt_For::NStmt_For(vector<NStmt*>&& initStmts, NExp* condExp, NExp* continueExp, vector<NStmt*>&& body)
    : initStmts(move(initStmts)), condExp(condExp), continueExp(continueExp), body(move(body))
{

}

NStmt_Continue::NStmt_Continue()
{

}

NStmt_Break::NStmt_Break()
{

}

NStmt_Return::NStmt_Return(NExp* exp)
    : exp(exp)
{

}

NStmt_Block::NStmt_Block(vector<NStmt*>&& stmts)
    : stmts(move(stmts))
{

}

NStmt_Blank::NStmt_Blank()
{

}

NStmt_Exp::NStmt_Exp(NExp* exp)
    : exp(exp)
{

}

NStmt_Task::NStmt_Task(NLambdaDecl* lambdaDecl, vector<NArgument>&& captureArgs)
    : lambdaDecl(lambdaDecl), captureArgs(move(captureArgs))
{
}

NStmt_Await::NStmt_Await(vector<NStmt*>&& body)
    : body(move(body))
{

}

NStmt_Async::NStmt_Async(NLambdaDecl* lambdaDecl, vector<NArgument>&& captureArgs)
    : lambdaDecl(lambdaDecl), captureArgs(move(captureArgs))
{

}

NStmt_Foreach::NStmt_Foreach(NExp* enumeratorExp, RType* itemType, const RName& varName, NExp* nextExp, vector<NStmt*>&& body)
    : enumeratorExp(enumeratorExp), itemType(itemType), varName(varName), nextExp(nextExp), body(move(body))
{

}

NStmt_ForeachCast::NStmt_ForeachCast(NExp* enumeratorExp, RType* itemType, const RName& varName, RType* rawItemType, NExp* nextExp, NExp* castExp, vector<NStmt*>&& body)
    : enumeratorExp(enumeratorExp), itemType(itemType), varName(varName), rawItemType(rawItemType), nextExp(nextExp), castExp(castExp), body(move(body))
{

}

NStmt_Yield::NStmt_Yield(NExp* value)
    : value(value)
{

}

NStmt_CallClassCtor::NStmt_CallClassCtor()
{

}

NStmt_CallStructCtor::NStmt_CallStructCtor()
{

}

NStmt_NullDirective::NStmt_NullDirective(NLoc* loc)
    : loc(loc)
{
}

NStmt_NotNullDirective::NStmt_NotNullDirective(NLoc* loc)
    : loc(loc)
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