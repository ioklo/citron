#pragma once

import <optional>;
import <vector>;
import <memory>;

namespace Citron {

using SStmtPtr = std::shared_ptr<class SStmt>;
using NStmtPtr = std::shared_ptr<class NStmt>;

namespace SyntaxIR0Translator {

class TranslationContext;

bool TranslateSBodyToNStmts(const std::vector<SStmtPtr>& stmts, std::vector<NStmtPtr>* outStmts, TranslationContext& context);

} // namespace SyntaxIR0Translator

} // namespace Citron