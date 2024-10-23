#pragma once

#include <memory>
#include <optional>
#include <vector>

namespace Citron {

class SVarDecl;
using RStmtPtr = std::shared_ptr<class RStmt>;

namespace SyntaxIR0Translator {

class TranslationContext;

bool TranslateSVarDeclToRStmts(SVarDecl& varDecl, std::vector<RStmtPtr>* outResult, TranslationContext& context);

} // namespace SyntaxIR0Translator

} // namespace Citron
