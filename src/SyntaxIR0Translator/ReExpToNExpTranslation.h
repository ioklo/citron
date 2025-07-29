#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"

namespace Citron {

class NExp;
using NExpPtr = std::shared_ptr<NExp>;

namespace SyntaxIR0Translator {

class TranslationContext;
class ReExp;

std::expected<NExpPtr, DiagPtr> TranslateReExpToNExp(ReExp& reExp, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron
