#pragma once

import <memory>;

namespace Citron {

class SExp;

namespace SyntaxIR0Translator {

using IrExpPtr = std::shared_ptr<class IrExp>;
class TranslationContext;

IrExpPtr TranslateSExpRefToIrExp(SExp& exp, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron