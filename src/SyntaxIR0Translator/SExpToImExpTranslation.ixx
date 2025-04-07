export module Citron.SyntaxIR0Translator:SExpToImExpTranslation;

import <memory>;
import <expected>;

import Citron.Diag;
import Citron.Syntax;
import Citron.RDecls;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;

export class ImExp;
export using ImExpPtr = std::shared_ptr<ImExp>;

export std::expected<ImExpPtr, DiagPtr> TranslateSExpToImExp(SExp& exp, const RTypePtr& hintType, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator
