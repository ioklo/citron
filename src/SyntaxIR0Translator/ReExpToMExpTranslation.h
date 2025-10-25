#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"

namespace Citron {

class MExp;

namespace SyntaxIR0Translator {

class TranslationContext;
class ReExp;

std::expected<MExp*, DiagPtr> TranslateReExpToMExp(ReExp* reExp, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron
