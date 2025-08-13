#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron::SyntaxIR0Translator {

class IrExp;
class TranslationContext;

std::expected<IrExp*, DiagPtr> TranslateSExpRefToIrExp(SExp& exp, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator
