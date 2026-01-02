#pragma once

#include <optional>
#include <span>
#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class MStmt;
struct TranslationContexts;

std::expected<void, DiagPtr> TranslateSBodyToMStmts(std::vector<MStmt*>* outStmts, std::span<SStmt*> stmts, TranslationContexts& contexts);
std::expected<std::vector<MStmt*>, DiagPtr> TranslateSBodyToMStmts(std::span<SStmt*> stmts, TranslationContexts& contexts);

} // namespace Citron