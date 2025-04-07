export module Citron.SyntaxIR0Translator:ReExpToNExpTranslation;

import <memory>;
import <expected>;

import Citron.NDecls;
import Citron.Diag;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;
export class ReExp;

export std::expected<NExpPtr, DiagPtr> TranslateReExpToNExp(ReExp& reExp, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator