#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"

namespace Citron {

class NExp;

namespace SyntaxIR0Translator {

class IrExp;
class TranslationContext;

std::expected<NExp*, DiagPtr> TranslateIrExpToNExp(IrExp* irExp, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron