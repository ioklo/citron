#pragma once

#include <memory>
#include <optional>
#include <vector>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

struct MStmt;
struct SmTranslationContexts;

std::expected<void, DiagPtr> TranslateSVarDeclToMStmts(std::vector<MStmt*>& outStmts, SVarDecl* varDecl, SmTranslationContexts& contexts);

} // namespace Citron
