#pragma once

#include <memory>
#include <optional>
#include <vector>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class NStmt;
using NStmtPtr = std::shared_ptr<NStmt>;

namespace SyntaxIR0Translator {

class TranslationContext;

std::expected<void, DiagPtr> TranslateSVarDeclToNStmts(std::vector<NStmtPtr>* outStmts, SVarDecl& varDecl, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron
