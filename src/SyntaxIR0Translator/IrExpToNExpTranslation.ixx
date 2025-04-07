export module Citron.SyntaxIR0Translator:IrExpToNExpTranslation;

import <memory>;
import <expected>;

import Citron.NDecls;
import Citron.Diag;

namespace Citron::SyntaxIR0Translator {

export class IrExp;
export class TranslationContext;

export std::expected<NExpPtr, DiagPtr> TranslateIrExpToNExp(IrExp& irExp, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator