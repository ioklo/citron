export module Citron.SyntaxIR0Translator:SStmtToNStmtTranslation;

import <optional>;
import <vector>;
import <memory>;

import Citron.Syntax;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;

export bool TranslateSBodyToNStmts(const std::vector<SStmtPtr>& stmts, std::vector<NStmtPtr>* outStmts, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator