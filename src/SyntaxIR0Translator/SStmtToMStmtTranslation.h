#pragma once

#include <optional>
#include <span>
#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class MStmt;

namespace SyntaxIR0Translator {

class TranslationContext;

std::expected<void, DiagPtr> TranslateSBodyToMStmts(std::vector<MStmt*>* outStmts, std::span<SStmt*> stmts, TranslationContext& context);
std::expected<std::vector<MStmt*>, DiagPtr> TranslateSBodyToMStmts(std::span<SStmt*> stmts, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron