#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron::SyntaxIR0Translator {

class IrExp;
using IrExpPtr = std::shared_ptr<IrExp>;

class TranslationContext;

std::expected<IrExpPtr, DiagPtr> TranslateSExpRefToIrExp(SExp& exp, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator
