#pragma once

#include <memory>
#include <optional>
#include <vector>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

struct MStmt;
struct TranslationContexts;

std::expected<void, DiagPtr> TranslateSVarDeclToMStmts(std::vector<MStmt*>* outStmts, SVarDecl* varDecl, TranslationContexts& contexts);

} // namespace Citron
