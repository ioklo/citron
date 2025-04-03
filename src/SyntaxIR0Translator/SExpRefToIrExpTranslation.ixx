export module Citron.SyntaxIR0Translator:SExpRefToIrExpTranslation;

import <memory>;

import Citron.Syntax;

namespace Citron::SyntaxIR0Translator {

export class IrExp;
export using IrExpPtr = std::shared_ptr<IrExp>;

export class TranslationContext;

IrExpPtr TranslateSExpRefToIrExp(SExp& exp, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator
