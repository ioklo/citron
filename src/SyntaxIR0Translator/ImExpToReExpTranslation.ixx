export module Citron.SyntaxIR0Translator:ImExpToReExpTranslation;

import <memory>;
import <expected>;

import Citron.Diag;

namespace Citron::SyntaxIR0Translator {

export class ReExp;
export using ReExpPtr = std::shared_ptr<ReExp>;

export class ImExp;
export class TranslationContext;

export std::expected<ReExpPtr, DiagPtr> TranslateImExpToReExp(ImExp& imExp, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator