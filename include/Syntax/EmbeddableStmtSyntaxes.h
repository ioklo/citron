#pragma once

#include "SyntaxConfig.h"

#include <variant>
#include <vector>
#include <memory>
#include <Infra/Json.h>

#include "SyntaxMacros.h"

namespace Citron {

// forward declarations
using StmtSyntax = std::variant <
    class CommandStmtSyntax,
    class VarDeclStmtSyntax,
    class IfStmtSyntax,
    class IfTestStmtSyntax,
    class ForStmtSyntax,

    class ContinueStmtSyntax,
    class BreakStmtSyntax,
    class ReturnStmtSyntax,
    class BlockStmtSyntax,
    class BlankStmtSyntax,
    class ExpStmtSyntax,

    class TaskStmtSyntax,
    class AwaitStmtSyntax,
    class AsyncStmtSyntax,
    class ForeachStmtSyntax,
    class YieldStmtSyntax,

    class DirectiveStmtSyntax
> ;

// recursive { (IfStmt)stmt.body }
class SingleEmbeddableStmtSyntax
{
    struct Impl;
    std::unique_ptr<Impl> impl;

public:
    SYNTAX_API SingleEmbeddableStmtSyntax(StmtSyntax stmt);
    DECLARE_DEFAULTS(SingleEmbeddableStmtSyntax);

    SYNTAX_API StmtSyntax& GetStmt();

    SYNTAX_API JsonItem ToJson();
};

class BlockEmbeddableStmtSyntax
{
    std::vector<StmtSyntax> stmts;

public:
    SYNTAX_API BlockEmbeddableStmtSyntax(std::vector<StmtSyntax> stmts);
    DECLARE_DEFAULTS(BlockEmbeddableStmtSyntax)

    std::vector<StmtSyntax>& GetStmts() { return stmts; }

    SYNTAX_API JsonItem ToJson();
};

using EmbeddableStmtSyntax = std::variant<SingleEmbeddableStmtSyntax, BlockEmbeddableStmtSyntax>;
SYNTAX_API JsonItem ToJson(EmbeddableStmtSyntax& syntax);

} // namespace Citron