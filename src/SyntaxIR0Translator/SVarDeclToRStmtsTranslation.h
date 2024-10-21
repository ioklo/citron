#pragma once

#include <memory>
#include <optional>
#include <vector>

namespace Citron {

using RStmtPtr = std::shared_ptr<class RStmt>;

namespace SyntaxIR0Translator {

class ScopeContext;

std::optional<std::vector<RStmtPtr>> TranslateSVarDeclToRStmts(SVarDecl& varDecl, TranslationContext& context);

} // namespace SyntaxIR0Translator

} // namespace Citron
