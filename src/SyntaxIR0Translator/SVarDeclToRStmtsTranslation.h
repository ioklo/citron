#pragma once

#include <memory>
#include <optional>
#include <vector>

namespace Citron {

using RStmtPtr = std::shared_ptr<class RStmt>;

namespace SyntaxIR0Translator {

class ScopeContext;

std::optional<std::vector<RStmtPtr>> TranslateSVarDeclToRStmt(SVarDecl& varDecl, ScopeContext& context);

} // namespace SyntaxIR0Translator

} // namespace Citron
