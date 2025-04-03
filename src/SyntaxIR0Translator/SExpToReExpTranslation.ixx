export module Citron.SyntaxIR0Translator:SExpToReExpTranslation;

import <memory>;

import Citron.Syntax;
import Citron.RDecls;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;
export class ReExp;
export using ReExpPtr = std::shared_ptr<ReExp>;

export ReExpPtr TranslateSExpToReExp(SExp& exp, const RTypePtr& hintType, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator