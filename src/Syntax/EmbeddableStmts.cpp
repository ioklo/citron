#include "pch.h"
#include "Syntax/EmbeddableStmts.h"

#include <memory>
#include "Syntax/Stmts.h"
#include "Syntax/SyntaxMacros.h"

using namespace std;

namespace Citron::Syntax {

struct SingleEmbeddableStmt::Impl
{
    Stmt stmt;
};

SingleEmbeddableStmt::SingleEmbeddableStmt(Stmt stmt)
    : impl(new Impl { std::move(stmt) })
{
}

IMPLEMENT_DEFAULTS_PIMPL(SingleEmbeddableStmt)

Stmt& SingleEmbeddableStmt::GetStmt()
{
    return impl->stmt;
}

BlockEmbeddableStmt::BlockEmbeddableStmt(vector<Stmt> stmts)
    : stmts(std::move(stmts))
{
}

IMPLEMENT_DEFAULTS_DEFAULT(BlockEmbeddableStmt)


}