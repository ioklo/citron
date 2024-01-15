#pragma once

#include "SyntaxConfig.h"

#include <variant>
#include <vector>
#include <memory>

#include "SyntaxMacros.h"

namespace Citron::Syntax {

// forward declarations
using Stmt = std::variant <
    class CommandStmt,
    class VarDeclStmt,
    class IfStmt,
    class IfTestStmt,
    class ForStmt,

    class ContinueStmt,
    class BreakStmt,
    class ReturnStmt,
    class BlockStmt,
    class BlankStmt,
    class ExpStmt,

    class TaskStmt,
    class AwaitStmt,
    class AsyncStmt,
    class ForeachStmt,
    class YieldStmt,

    class DirectiveStmt
> ;

// recursive { (IfStmt)stmt.body }
class SingleEmbeddableStmt
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API SingleEmbeddableStmt(Stmt stmt);
    DECLARE_DEFAULTS(SingleEmbeddableStmt);

    SYNTAX_API Stmt& GetStmt();
};

class BlockEmbeddableStmt
{
    std::vector<Stmt> stmts;

public:
    SYNTAX_API BlockEmbeddableStmt(std::vector<Stmt> stmts);
    DECLARE_DEFAULTS(BlockEmbeddableStmt)

    std::vector<Stmt>& GetStmts() { return stmts; }
};

using EmbeddableStmt = std::variant<SingleEmbeddableStmt, BlockEmbeddableStmt>;

} // namespace Citron::Syntax