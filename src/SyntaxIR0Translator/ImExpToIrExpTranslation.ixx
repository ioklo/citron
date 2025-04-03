export module Citron.SyntaxIR0Translator:ImExpToIrExpTranslation;

import <memory>;

import Citron.RDecls;

namespace Citron::SyntaxIR0Translator {

export class ImExp;
export using ImExpPtr = std::shared_ptr<ImExp>;

export class IrExp;
export using IrExpPtr = std::shared_ptr<IrExp>;

export class TranslationContext;

export IrExpPtr TranslateImExpToIrExp(const ImExpPtr& imExp, TranslationContext& context, RTypeFactory& factory);

} // namespace Citron::SyntaxIR0Translator