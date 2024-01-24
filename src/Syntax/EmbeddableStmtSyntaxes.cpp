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

BlockEmbeddableStmtSyntax::BlockEmbeddableStmtSyntax(vector<StmtSyntax> stmts)
    : stmts(std::move(stmts))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(BlockEmbeddableStmtSyntax)


}