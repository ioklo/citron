#pragma once

#include <optional>
#include <vector>
#include <memory>

namespace Citron {

class RStmt;
using RStmtPtr = std::shared_ptr<RStmt>;

class SStmt;
using SStmtPtr = std::shared_ptr<SStmt>;

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

namespace SyntaxIR0Translator {

std::optional<std::vector<RStmtPtr>> TranslateBody(std::vector<SStmtPtr>& stmts, ScopeContextPtr context);

} // namespace SyntaxIR0Translator

} // namespace Citron