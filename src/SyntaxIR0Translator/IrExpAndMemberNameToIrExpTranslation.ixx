export module Citron.SyntaxIR0Translator:IrExpAndMemberNameToIrExpTranslation;

import <memory>;
import <string>;
import <expected>;

import Citron.RDecls;
import Citron.Diag;

namespace Citron::SyntaxIR0Translator {

export class IrExp;
export using IrExpPtr = std::shared_ptr<IrExp>;

export class TranslationContext;

export std::expected<IrExpPtr, DiagPtr> TranslateIrExpAndMemberNameToIrExp(const IrExpPtr& irExp, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator 