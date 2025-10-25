#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"

namespace Citron {

class MExp;

namespace SyntaxIR0Translator {

class IrExp;
class TranslationContext;

std::expected<MExp*, DiagPtr> TranslateIrExpToMExp(IrExp* irExp, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron