#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"

namespace Citron {

class NExp;

namespace SyntaxIR0Translator {

class TranslationContext;
class ReExp;

std::expected<NExp*, DiagPtr> TranslateReExpToNExp(ReExp* reExp, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron
