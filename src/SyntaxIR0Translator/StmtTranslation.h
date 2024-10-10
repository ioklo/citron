#pragma once

#include <optional>
#include <vector>
#include <memory>

namespace Citron {

using RStmtPtr = std::shared_ptr<class RStmt>;
using SStmtPtr = std::shared_ptr<class SStmt>;
using ScopeContextPtr = std::shared_ptr<class ScopeContext>;

namespace SyntaxIR0Translator {

std::optional<std::vector<RStmtPtr>> TranslateBody(std::vector<SStmtPtr>& stmts, ScopeContextPtr context);

} // namespace SyntaxIR0Translator

} // namespace Citron