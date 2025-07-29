#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"

namespace Citron {

class NExp;
using NExpPtr = std::shared_ptr<NExp>;

namespace SyntaxIR0Translator {

class IrExp;
class TranslationContext;

std::expected<NExpPtr, DiagPtr> TranslateIrExpToNExp(IrExp& irExp, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron