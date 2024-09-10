#pragma once

#include <optional>
#include <vector>
#include <memory>

namespace Citron {

class RStmt;
using RStmtPtr = std::unique_ptr<RStmt>;

class SStmt;
using SStmtPtr = std::shared_ptr<SStmt>;

class ScopeContext;

std::optional<std::vector<RStmtPtr>> TranslateBody(std::vector<SStmtPtr>& stmts, ScopeContext& context);

}
