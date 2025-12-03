#pragma once

#include <optional>
#include <span>
#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class MStmt;

class TranslationContext;

std::expected<void, DiagPtr> TranslateSBodyToMStmts(std::vector<MStmt*>* outStmts, std::span<SStmt*> stmts, TranslationContext& context);
std::expected<std::vector<MStmt*>, DiagPtr> TranslateSBodyToMStmts(std::span<SStmt*> stmts, TranslationContext& context);

} // namespace Citron