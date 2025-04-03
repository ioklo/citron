export module Citron.SyntaxIR0Translator:SExpRefToNExpTranslation;

import <memory>;

import Citron.Syntax;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;

export NExpPtr TranslateSExpRefToNExp(SExp& exp, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator
