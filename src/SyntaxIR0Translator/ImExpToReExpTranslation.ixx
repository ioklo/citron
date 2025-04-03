export module Citron.SyntaxIR0Translator:ImExpToReExpTranslation;

import <memory>;

namespace Citron::SyntaxIR0Translator {

export class ReExp;
export using ReExpPtr = std::shared_ptr<ReExp>;
export class ImExp;
export class TranslationContext;

export ReExpPtr TranslateImExpToReExp(ImExp& imExp, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator