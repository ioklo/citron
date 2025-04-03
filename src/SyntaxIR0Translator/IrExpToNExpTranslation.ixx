export module Citron.SyntaxIR0Translator:IrExpToNExpTranslation;

import <memory>;

import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class IrExp;
export class TranslationContext;

export NExpPtr TranslateIrExpToNExp(IrExp& irExp, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator