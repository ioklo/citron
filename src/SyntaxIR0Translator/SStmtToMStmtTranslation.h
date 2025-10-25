#pragma once

#include <optional>
#include <vector>
#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class MStmt;

namespace SyntaxIR0Translator {

class TranslationContext;

std::expected<void, DiagPtr> TranslateSBodyToMStmts(std::vector<MStmt*>* outStmts, const std::vector<SStmt*>& stmts, TranslationContext& context);
std::expected<std::vector<MStmt*>, DiagPtr> TranslateSBodyToMStmts(const std::vector<SStmt*>& stmts, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron