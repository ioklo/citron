export module Citron.SyntaxIR0Translator:SExpToImExpTranslation;

import <memory>;

import Citron.Syntax;
import Citron.RDecls;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;

export class ImExp;
export using ImExpPtr = std::shared_ptr<ImExp>;

export ImExpPtr TranslateSExpToImExp(SExp& exp, const RTypePtr& hintType, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator
