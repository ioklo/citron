#include "RStmt.h"

#include "RExp.h"

namespace Citron {


RStmt_Command::RStmt_Command(std::vector<RExp_String>&& commands)
    : commands(std::move(commands))
{
}

RStmt_LocalVarDecl::RStmt_LocalVarDecl()
{

}

RStmt_If::RStmt_If()
{

}

RStmt_IfNullableRefTest::RStmt_IfNullableRefTest(RTypePtr&& refType, RName&& varName, RExpPtr&& asExp, std::vector<RStmtPtr>&& body, std::vector<RStmtPtr>&& elseBody)
    : refType(std::move(refType)), varName(std::move(varName)), asExp(std::move(asExp)), body(std::move(body)), elseBody(std::move(elseBody))
{

}

RStmt_IfNullableValueTest::RStmt_IfNullableValueTest()
{

}

RStmt_For::RStmt_For(std::vector<RStmtPtr>&& initStmts, RExpPtr&& condExp, RExpPtr&& continueExp, std::vector<RStmtPtr>&& body)
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

RStmt_Block::RStmt_Block(std::vector<RStmtPtr>&& stmts)
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

RStmt_Task::RStmt_Task(std::shared_ptr<RLambdaDecl>&& lambdaDecl, std::vector<RArgument>&& captureArgs)
    : lambdaDecl(std::move(lambdaDecl)), captureArgs(std::move(captureArgs))
{
}

RStmt_Await::RStmt_Await(std::vector<RStmtPtr>&& body)
    : body(std::move(body))
{

}

RStmt_Async::RStmt_Async(std::shared_ptr<RLambdaDecl>&& lambdaDecl, std::vector<RArgument>&& captureArgs)
    : lambdaDecl(std::move(lambdaDecl)), captureArgs(std::move(captureArgs))
{

}

RStmt_Foreach::RStmt_Foreach()
{

}

RStmt_ForeachCast::RStmt_ForeachCast()
{

}

RStmt_Yield::RStmt_Yield()
{

}

RStmt_CallClassConstructor::RStmt_CallClassConstructor()
{

}

RStmt_CallStructConstructor::RStmt_CallStructConstructor()
{

}

RStmt_NullDirective::RStmt_NullDirective()
{

}

RStmt_NotNullDirective::RStmt_NotNullDirective()
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