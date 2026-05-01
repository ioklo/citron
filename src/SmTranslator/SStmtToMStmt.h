#pragma once

#include <optional>
#include <span>
#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

struct MStmt;
struct MStmt_Scope;
struct TranslationContexts;

std::expected<void, DiagPtr> TranslateSStmtsToMStmts(std::vector<MStmt*>& outStmts, std::span<SStmt*> stmts, TranslationContexts& contexts);
std::expected<MStmt_Scope*, DiagPtr> TranslateScopedSStmtsToMStmt_Scope(std::span<SStmt*> stmts, TranslationContexts& contexts);

} // namespace Citron