#pragma once

#include <optional>
#include <vector>
#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class NStmt;

namespace SyntaxIR0Translator {

class TranslationContext;

std::expected<void, DiagPtr> TranslateSBodyToNStmts(std::vector<NStmt*>* outStmts, const std::vector<SStmtPtr>& stmts, TranslationContext& context);
std::expected<std::vector<NStmt*>, DiagPtr> TranslateSBodyToNStmts(const std::vector<SStmtPtr>& stmts, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron