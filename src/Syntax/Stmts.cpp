#include "pch.h"
#include "Syntax/Stmts.h"

using namespace std;

namespace Citron::Syntax {

CommandStmt::CommandStmt(vector<StringExp> commands)
    : commands(std::move(commands))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(CommandStmt)

struct IfStmt::Impl
{
    Exp cond;
    EmbeddableStmt body; 
    optional<EmbeddableStmt> elseBody;
};

IfStmt::IfStmt(Exp cond, EmbeddableStmt body, optional<EmbeddableStmt> elseBody)
    : impl(new Impl { std::move(cond), std::move(body), std::move(elseBody) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(IfStmt)

Exp& IfStmt::GetCond()
{
    return impl->cond;
}

EmbeddableStmt& IfStmt::GetBody()
{
    return impl->body;
}

optional<EmbeddableStmt>& IfStmt::GetElseBody()
{
    return impl->elseBody;
}

struct IfTestStmt::Impl 
{
    TypeExp testTypeExp;
    string varName;
    Exp exp; 
    EmbeddableStmt body;
    optional<EmbeddableStmt> elseBody;
};

IfTestStmt::IfTestStmt(TypeExp testTypeExp, string varName, Exp exp, EmbeddableStmt body, optional<EmbeddableStmt> elseBody)
    : impl(new Impl { std::move(testTypeExp), std::move(varName), std::move(exp), std::move(body), std::move(elseBody) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(IfTestStmt)

TypeExp& IfTestStmt::GetTestTypeExp()
{
    return impl->testTypeExp;
}

string& IfTestStmt::GetVarName()
{
    return impl->varName;
}

Exp& IfTestStmt::GetExp()
{
    return impl->exp;
}

EmbeddableStmt& IfTestStmt::GetBody()
{
    return impl->body;
}

optional<EmbeddableStmt>& IfTestStmt::GetElseBody()
{
    return impl->elseBody;
}

struct ForStmt::Impl
{
    optional<ForStmtInitializer> initializer;
    optional<Exp> condExp;
    optional<Exp> continueExp;
    EmbeddableStmt body;
};

ForStmt::ForStmt(optional<ForStmtInitializer> initializer, optional<Exp> condExp, optional<Exp> continueExp, EmbeddableStmt body)
    : impl(new Impl { std::move(initializer), std::move(condExp), std::move(continueExp), std::move(body) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(ForStmt)

optional<ForStmtInitializer>& ForStmt::GetInitializer()
{
    return impl->initializer;
}

optional<Citron::Syntax::Exp>& ForStmt::GetCondExp()
{
    return impl->condExp;
}

optional<Citron::Syntax::Exp>& ForStmt::GetContinueExp()
{
    return impl->continueExp;
}

EmbeddableStmt& ForStmt::GetBody()
{
    return impl->body;
}

ReturnStmt::ReturnStmt(optional<ReturnValueInfo> info)
    : info(std::move(info))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(ReturnStmt)

BlockStmt::BlockStmt(vector<Stmt> stmts)
    : stmts(std::move(stmts))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(BlockStmt)

TaskStmt::TaskStmt(vector<Stmt> body)
    : body(std::move(body))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(TaskStmt)


AwaitStmt::AwaitStmt(vector<Stmt> body)
    : body(std::move(body))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(AwaitStmt)

AsyncStmt::AsyncStmt(vector<Stmt> body)
    : body(std::move(body))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(AsyncStmt)

struct ForeachStmt::Impl
{
    TypeExp type;
    string varName;
    Exp enumerable;
    EmbeddableStmt body;
};

ForeachStmt::ForeachStmt(TypeExp type, string varName, Exp enumerable, EmbeddableStmt body)
    : impl(new Impl { std::move(type), std::move(varName), std::move(enumerable), std::move(body) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(ForeachStmt)

TypeExp& ForeachStmt::GetType()
{
    return impl->type;
}

string& ForeachStmt::GetVarName()
{
    return impl->varName;
}

Exp& ForeachStmt::GetEnumerable()
{
    return impl->enumerable;
}

EmbeddableStmt& ForeachStmt::GetBody()
{
    return impl->body;
}

}