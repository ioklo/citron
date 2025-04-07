export module Citron.SyntaxIR0Translator:SExpRefToIrExpTranslation;

import <memory>;
import <expected>;

import Citron.Diag;
import Citron.Syntax;

namespace Citron::SyntaxIR0Translator {

export class IrExp;
export using IrExpPtr = std::shared_ptr<IrExp>;

export class TranslationContext;

export std::expected<IrExpPtr, DiagPtr> TranslateSExpRefToIrExp(SExp& exp, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator
