export module Citron.SyntaxIR0Translator:SVarDeclToNStmtsTranslation;

import <memory>;
import <optional>;
import <vector>;
import <expected>;

import Citron.Diag;
import Citron.Syntax;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;

export std::expected<std::vector<NStmtPtr>, DiagPtr> TranslateSVarDeclToNStmts(SVarDecl& varDecl, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator
