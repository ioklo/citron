#include "pch.h"
#include <Syntax/StmtSyntaxes.h>

using namespace std;

namespace Citron {

CommandStmtSyntax::CommandStmtSyntax(vector<StringExpSyntax> commands)
    : commands(std::move(commands))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(CommandStmtSyntax)

BEGIN_IMPLEMENT_JSON_CLASS(CommandStmtSyntax)
    IMPLEMENT_JSON_MEMBER(commands)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(VarDeclStmtSyntax)
    IMPLEMENT_JSON_MEMBER(varDecl)
END_IMPLEMENT_JSON_CLASS()

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

BEGIN_IMPLEMENT_JSON_CLASS(IfStmtSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, cond)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, body)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, elseBody)
END_IMPLEMENT_JSON_CLASS()

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

BEGIN_IMPLEMENT_JSON_CLASS(IfTestStmtSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, testTypeExp)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, varName)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, exp)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, body)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, elseBody)
END_IMPLEMENT_JSON_CLASS()

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

BEGIN_IMPLEMENT_JSON_CLASS(ForStmtSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, initializer)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, condExp)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, continueExp)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, body)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(ContinueStmtSyntax)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(BreakStmtSyntax)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_STRUCT(ReturnValueSyntaxInfo, syntax)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, value)
END_IMPLEMENT_JSON_STRUCT()

ReturnStmtSyntax::ReturnStmtSyntax(optional<ReturnValueSyntaxInfo> info)
    : info(std::move(info))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(ReturnStmtSyntax)

BEGIN_IMPLEMENT_JSON_CLASS(ReturnStmtSyntax)
    IMPLEMENT_JSON_MEMBER(info)
END_IMPLEMENT_JSON_CLASS()

BlockStmtSyntax::BlockStmtSyntax(vector<StmtSyntax> stmts)
    : stmts(std::move(stmts))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(BlockStmtSyntax)

BEGIN_IMPLEMENT_JSON_CLASS(BlockStmtSyntax)
    IMPLEMENT_JSON_MEMBER(stmts)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(BlankStmtSyntax)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(ExpStmtSyntax)
    IMPLEMENT_JSON_MEMBER(exp)
END_IMPLEMENT_JSON_CLASS()

TaskStmtSyntax::TaskStmtSyntax(vector<StmtSyntax> body)
    : body(std::move(body))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(TaskStmtSyntax)

BEGIN_IMPLEMENT_JSON_CLASS(TaskStmtSyntax)
    IMPLEMENT_JSON_MEMBER(body)
END_IMPLEMENT_JSON_CLASS()

AwaitStmtSyntax::AwaitStmtSyntax(vector<StmtSyntax> body)
    : body(std::move(body))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(AwaitStmtSyntax)

BEGIN_IMPLEMENT_JSON_CLASS(AwaitStmtSyntax)
    IMPLEMENT_JSON_MEMBER(body)
END_IMPLEMENT_JSON_CLASS()

AsyncStmtSyntax::AsyncStmtSyntax(vector<StmtSyntax> body)
    : body(std::move(body))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(AsyncStmtSyntax)

BEGIN_IMPLEMENT_JSON_CLASS(AsyncStmtSyntax)
    IMPLEMENT_JSON_MEMBER(body)
END_IMPLEMENT_JSON_CLASS()

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

BEGIN_IMPLEMENT_JSON_CLASS(ForeachStmtSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, type)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, varName)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, enumerable)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, body)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(YieldStmtSyntax)
    IMPLEMENT_JSON_MEMBER(value)
END_IMPLEMENT_JSON_CLASS()

BEGIN_IMPLEMENT_JSON_CLASS(DirectiveStmtSyntax)
    IMPLEMENT_JSON_MEMBER(name)
    IMPLEMENT_JSON_MEMBER(args)
END_IMPLEMENT_JSON_CLASS()

JsonItem ToJson(StmtSyntax& syntax)
{
    return std::visit([](auto&& stmt) { return stmt.ToJson(); }, syntax);
}

}