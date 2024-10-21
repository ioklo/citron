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

class TranslationContext;

std::optional<std::vector<RStmtPtr>> TranslateSBodyToRStmts(const std::vector<SStmtPtr>& stmts, TranslationContext& context);

} // namespace SyntaxIR0Translator

} // namespace Citron