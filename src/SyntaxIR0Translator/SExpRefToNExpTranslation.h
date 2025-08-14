#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class NExp;

namespace SyntaxIR0Translator {

class TranslationContext;

std::expected<NExp*, DiagPtr> TranslateSExpRefToNExp(SExp* exp, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron
