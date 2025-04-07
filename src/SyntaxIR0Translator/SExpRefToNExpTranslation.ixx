export module Citron.SyntaxIR0Translator:SExpRefToNExpTranslation;

import <memory>;
import <expected>;

import Citron.Diag;
import Citron.Syntax;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;

export std::expected<NExpPtr, DiagPtr> TranslateSExpRefToNExp(SExp& exp, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator
