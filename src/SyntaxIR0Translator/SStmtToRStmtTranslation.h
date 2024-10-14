#pragma once

#include <optional>
#include <vector>
#include <memory>

namespace Citron {

class RTypeFactory;
class Logger;

using SStmtPtr = std::shared_ptr<class SStmt>;
using RStmtPtr = std::shared_ptr<class RStmt>;


namespace SyntaxIR0Translator {

using ScopeContextPtr = std::shared_ptr<class ScopeContext>;


std::optional<std::vector<RStmtPtr>> TranslateSBodyToRStmts(const std::vector<SStmtPtr>& stmts, const ScopeContextPtr& context, Logger& logger, RTypeFactory& factory);

} // namespace SyntaxIR0Translator

} // namespace Citron