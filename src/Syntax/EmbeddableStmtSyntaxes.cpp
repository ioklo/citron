#include "pch.h"
#include <Syntax/EmbeddableStmtSyntaxes.h>

#include <memory>
#include <Syntax/StmtSyntaxes.h>
#include <Syntax/SyntaxMacros.h>

using namespace std;

namespace Citron {

struct SingleEmbeddableStmtSyntax::Impl
{
    StmtSyntax stmt;
};

SingleEmbeddableStmtSyntax::SingleEmbeddableStmtSyntax(StmtSyntax stmt)
    : impl(new Impl { std::move(stmt) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(SingleEmbeddableStmtSyntax)

StmtSyntax& SingleEmbeddableStmtSyntax::GetStmt()
{
    return impl->stmt;
}

BEGIN_IMPLEMENT_JSON_CLASS(SingleEmbeddableStmtSyntax)
    IMPLEMENT_JSON_MEMBER_INDIRECT(impl, stmt)
END_IMPLEMENT_JSON_CLASS()

BlockEmbeddableStmtSyntax::BlockEmbeddableStmtSyntax(vector<StmtSyntax> stmts)
    : stmts(std::move(stmts))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(BlockEmbeddableStmtSyntax)

BEGIN_IMPLEMENT_JSON_CLASS(BlockEmbeddableStmtSyntax)
    IMPLEMENT_JSON_MEMBER(stmts)
END_IMPLEMENT_JSON_CLASS()

JsonItem ToJson(EmbeddableStmtSyntax& syntax)
{
    return std::visit([](auto&& embeddableStmt) { return embeddableStmt.ToJson(); }, syntax);
}

}