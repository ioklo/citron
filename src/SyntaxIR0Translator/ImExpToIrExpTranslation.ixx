export module Citron.SyntaxIR0Translator:ImExpToIrExpTranslation;

import <memory>;
import <expected>; 

import Citron.RDecls;
import Citron.Diag;

namespace Citron::SyntaxIR0Translator {

export class ImExp;
export using ImExpPtr = std::shared_ptr<ImExp>;

export class IrExp;
export using IrExpPtr = std::shared_ptr<IrExp>;

export class TranslationContext;

export std::expected<IrExpPtr, DiagPtr> TranslateImExpToIrExp(const ImExpPtr& imExp, TranslationContext& context, RTypeFactory& factory);

} // namespace Citron::SyntaxIR0Translator