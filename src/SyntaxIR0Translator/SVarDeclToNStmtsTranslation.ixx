export module Citron.SyntaxIR0Translator:SVarDeclToNStmtsTranslation;

import <memory>;
import <optional>;
import <vector>;

import Citron.Syntax;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;

bool TranslateSVarDeclToNStmts(SVarDecl& varDecl, std::vector<NStmtPtr>* outResult, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator
