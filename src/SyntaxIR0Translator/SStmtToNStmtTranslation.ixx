export module Citron.SyntaxIR0Translator:SStmtToNStmtTranslation;

import <optional>;
import <vector>;
import <memory>;
import <expected>;

import Citron.Diag;
import Citron.Syntax;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;

export std::expected<void, DiagPtr> TranslateSBodyToNStmts(std::vector<NStmtPtr>* outStmts, const std::vector<SStmtPtr>& stmts, TranslationContext& context);
export std::expected<std::vector<NStmtPtr>, DiagPtr> TranslateSBodyToNStmts(const std::vector<SStmtPtr>& stmts, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator