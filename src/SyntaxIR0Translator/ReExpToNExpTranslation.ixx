export module Citron.SyntaxIR0Translator:ReExpToNExpTranslation;

import <memory>;

import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;
export class ReExp;

export NExpPtr TranslateReExpToNExp(ReExp& reExp, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator