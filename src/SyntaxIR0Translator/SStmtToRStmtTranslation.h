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

bool TranslateSBodyToRStmts(const std::vector<SStmtPtr>& stmts, std::vector<RStmtPtr>* outStmts, TranslationContext& context);

} // namespace SyntaxIR0Translator

} // namespace Citron