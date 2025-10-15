#pragma once

#include <memory>
#include <optional>
#include <vector>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class NStmt;

namespace SyntaxIR0Translator {

class TranslationContext;

std::expected<void, DiagPtr> TranslateSVarDeclToNStmts(std::vector<NStmt*>* outStmts, SVarDecl* varDecl, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron
