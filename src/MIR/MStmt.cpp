#include "MStmt.h"
#include "MExp.h"

using namespace std;

namespace Citron {

void MStmt_Command::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_LocalVarDecl::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_LocalRefDecl::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_If::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_IfNullableRefTest::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_IfNullableValueTest::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_For::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Continue::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Break::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Return::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Block::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Blank::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Exp::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Task::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Await::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Async::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Foreach::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_ForeachCast::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_Yield::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_CallClassCtor::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_CallStructCtor::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_NullDirective::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_NotNullDirective::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_StaticNullDirective::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_StaticNotNullDirective::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }
void MStmt_StaticUnknownNullDirective::Accept(MStmtVisitor& visitor) { visitor.Visit(this); }

MStmt_Command::MStmt_Command(vector<MExp_String*>&& commands)
    : commands(move(commands))
{
}

MStmt_LocalVarDecl::MStmt_LocalVarDecl(RType* type, const RName& name, MStmt_LocalVarDeclInit init)
    : type{type}, name{name}, init{init}
{

}

MStmt_LocalRefDecl::MStmt_LocalRefDecl(RType* type, RName&& name, MLoc* loc)
    : type{type}, name{move(name)}, loc{loc}
{
    
}

MStmt_If::MStmt_If(MExp* cond, vector<MStmt*>&& body, vector<MStmt*>&& elseBody)
    : cond{cond}, body{move(body)}, elseBody(move(elseBody))
{

}

MStmt_IfNullableRefTest::MStmt_IfNullableRefTest(RType* refType, RName&& varName, MExp* asExp, vector<MStmt*>&& body, vector<MStmt*>&& elseBody)
    : refType(refType), varName(move(varName)), asExp(asExp), body(move(body)), elseBody(move(elseBody))
{

}

MStmt_IfNullableValueTest::MStmt_IfNullableValueTest(RType* type, RName&& varName, MExp* asExp, vector<MStmt*>&& body, vector<MStmt*>&& elseBody)
    : type(type), varName(move(varName)), asExp(asExp), body(move(body)), elseBody(move(elseBody))
{

}

MStmt_For::MStmt_For(vector<MStmt*>&& initStmts, MExp* condExp, MExp* continueExp, vector<MStmt*>&& body)
    : initStmts(move(initStmts)), condExp(condExp), continueExp(continueExp), body(move(body))
{

}

MStmt_Continue::MStmt_Continue()
{

}

MStmt_Break::MStmt_Break()
{

}

MStmt_Return::MStmt_Return(MExp* exp)
    : exp(exp)
{

}

MStmt_Block::MStmt_Block(vector<MStmt*>&& stmts)
    : stmts(move(stmts))
{

}

MStmt_Blank::MStmt_Blank()
{

}

MStmt_Exp::MStmt_Exp(MExp* exp)
    : exp(exp)
{

}

MStmt_Task::MStmt_Task(NLambdaDecl* lambdaDecl, vector<MArgument>&& captureArgs)
    : lambdaDecl(lambdaDecl), captureArgs(move(captureArgs))
{
}

MStmt_Await::MStmt_Await(vector<MStmt*>&& body)
    : body(move(body))
{

}

MStmt_Async::MStmt_Async(NLambdaDecl* lambdaDecl, vector<MArgument>&& captureArgs)
    : lambdaDecl(lambdaDecl), captureArgs(move(captureArgs))
{

}

MStmt_Foreach::MStmt_Foreach(MExp* enumeratorExp, RType* itemType, const RName& varName, MExp* nextExp, vector<MStmt*>&& body)
    : enumeratorExp(enumeratorExp), itemType(itemType), varName(varName), nextExp(nextExp), body(move(body))
{

}

MStmt_ForeachCast::MStmt_ForeachCast(MExp* enumeratorExp, RType* itemType, const RName& varName, RType* rawItemType, MExp* nextExp, MExp* castExp, vector<MStmt*>&& body)
    : enumeratorExp(enumeratorExp), itemType(itemType), varName(varName), rawItemType(rawItemType), nextExp(nextExp), castExp(castExp), body(move(body))
{

}

MStmt_Yield::MStmt_Yield(MExp* value)
    : value(value)
{

}

MStmt_CallClassCtor::MStmt_CallClassCtor(RClassCtorDecl* ctor, vector<MArgument>&& args)
    : ctor{ctor}, args{move(args)}
{
}

MStmt_CallStructCtor::MStmt_CallStructCtor(NStructCtorDecl* ctor, RTypeArguments* typeArgs, vector<MArgument>&& args)
    : ctor{ctor}, typeArgs{typeArgs}, args{move(args)}
{
}

MStmt_NullDirective::MStmt_NullDirective(MLoc* loc)
    : loc(loc)
{
}

MStmt_NotNullDirective::MStmt_NotNullDirective(MLoc* loc)
    : loc{loc}
{

}

MStmt_StaticNullDirective::MStmt_StaticNullDirective(MLoc* loc)
    : loc{loc}
{

}

MStmt_StaticNotNullDirective::MStmt_StaticNotNullDirective(MLoc* loc)
    : loc{loc}
{

}

MStmt_StaticUnknownNullDirective::MStmt_StaticUnknownNullDirective(MLoc* loc)
    : loc{loc}
{

}

}