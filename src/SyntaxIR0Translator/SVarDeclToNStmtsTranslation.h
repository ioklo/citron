#pragma once

#include <memory>
#include <optional>
#include <vector>

namespace Citron {

class SVarDecl;
using NStmtPtr = std::shared_ptr<class NStmt>;

namespace SyntaxIR0Translator {

class TranslationContext;

bool TranslateSVarDeclToNStmts(SVarDecl& varDecl, std::vector<NStmtPtr>* outResult, TranslationContext& context);

} // namespace SyntaxIR0Translator

} // namespace Citron
