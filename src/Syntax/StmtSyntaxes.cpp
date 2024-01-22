#include "pch.h"
#include "Syntax/StmtSyntaxes.h"

using namespace std;

namespace Citron {

CommandStmtSyntax::CommandStmtSyntax(vector<StringExpSyntax> commands)
    : commands(std::move(commands))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(CommandStmtSyntax)

struct IfStmtSyntax::Impl
{
    ExpSyntax cond;
    EmbeddableStmtSyntax body; 
    optional<EmbeddableStmtSyntax> elseBody;
};

IfStmtSyntax::IfStmtSyntax(ExpSyntax cond, EmbeddableStmtSyntax body, optional<EmbeddableStmtSyntax> elseBody)
    : impl(new Impl { std::move(cond), std::move(body), std::move(elseBody) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(IfStmtSyntax)

ExpSyntax& IfStmtSyntax::GetCond()
{
    return impl->cond;
}

EmbeddableStmtSyntax& IfStmtSyntax::GetBody()
{
    return impl->body;
}

optional<EmbeddableStmtSyntax>& IfStmtSyntax::GetElseBody()
{
    return impl->elseBody;
}

struct IfTestStmtSyntax::Impl 
{
    TypeExpSyntax testTypeExp;
    u32string varName;
    ExpSyntax exp; 
    EmbeddableStmtSyntax body;
    optional<EmbeddableStmtSyntax> elseBody;
};

IfTestStmtSyntax::IfTestStmtSyntax(TypeExpSyntax testTypeExp, u32string varName, ExpSyntax exp, EmbeddableStmtSyntax body, optional<EmbeddableStmtSyntax> elseBody)
    : impl(new Impl { std::move(testTypeExp), std::move(varName), std::move(exp), std::move(body), std::move(elseBody) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(IfTestStmtSyntax)

TypeExpSyntax& IfTestStmtSyntax::GetTestTypeExp()
{
    return impl->testTypeExp;
}

u32string& IfTestStmtSyntax::GetVarName()
{
    return impl->varName;
}

ExpSyntax& IfTestStmtSyntax::GetExp()
{
    return impl->exp;
}

EmbeddableStmtSyntax& IfTestStmtSyntax::GetBody()
{
    return impl->body;
}

optional<EmbeddableStmtSyntax>& IfTestStmtSyntax::GetElseBody()
{
    return impl->elseBody;
}

struct ForStmtSyntax::Impl
{
    optional<ForStmtInitializerSyntax> initializer;
    optional<ExpSyntax> condExp;
    optional<ExpSyntax> continueExp;
    EmbeddableStmtSyntax body;
};

ForStmtSyntax::ForStmtSyntax(optional<ForStmtInitializerSyntax> initializer, optional<ExpSyntax> condExp, optional<ExpSyntax> continueExp, EmbeddableStmtSyntax body)
    : impl(new Impl { std::move(initializer), std::move(condExp), std::move(continueExp), std::move(body) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(ForStmtSyntax)

optional<ForStmtInitializerSyntax>& ForStmtSyntax::GetInitializer()
{
    return impl->initializer;
}

optional<ExpSyntax>& ForStmtSyntax::GetCondExp()
{
    return impl->condExp;
}

optional<ExpSyntax>& ForStmtSyntax::GetContinueExp()
{
    return impl->continueExp;
}

EmbeddableStmtSyntax& ForStmtSyntax::GetBody()
{
    return impl->body;
}

ReturnStmtSyntax::ReturnStmtSyntax(optional<ReturnValueInfoSyntax> info)
    : info(std::move(info))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(ReturnStmtSyntax)

BlockStmtSyntax::BlockStmtSyntax(vector<StmtSyntax> stmts)
    : stmts(std::move(stmts))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(BlockStmtSyntax)

TaskStmtSyntax::TaskStmtSyntax(vector<StmtSyntax> body)
    : body(std::move(body))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(TaskStmtSyntax)


AwaitStmtSyntax::AwaitStmtSyntax(vector<StmtSyntax> body)
    : body(std::move(body))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(AwaitStmtSyntax)

AsyncStmtSyntax::AsyncStmtSyntax(vector<StmtSyntax> body)
    : body(std::move(body))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(AsyncStmtSyntax)

struct ForeachStmtSyntax::Impl
{
    TypeExpSyntax type;
    u32string varName;
    ExpSyntax enumerable;
    EmbeddableStmtSyntax body;
};

ForeachStmtSyntax::ForeachStmtSyntax(TypeExpSyntax type, u32string varName, ExpSyntax enumerable, EmbeddableStmtSyntax body)
    : impl(new Impl { std::move(type), std::move(varName), std::move(enumerable), std::move(body) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(ForeachStmtSyntax)

TypeExpSyntax& ForeachStmtSyntax::GetType()
{
    return impl->type;
}

u32string& ForeachStmtSyntax::GetVarName()
{
    return impl->varName;
}

ExpSyntax& ForeachStmtSyntax::GetEnumerable()
{
    return impl->enumerable;
}

EmbeddableStmtSyntax& ForeachStmtSyntax::GetBody()
{
    return impl->body;
}

}