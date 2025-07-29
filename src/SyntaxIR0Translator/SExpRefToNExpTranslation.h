#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class NExp;
using NExpPtr = std::shared_ptr<NExp>;

namespace SyntaxIR0Translator {

class TranslationContext;

std::expected<NExpPtr, DiagPtr> TranslateSExpRefToNExp(SExp& exp, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron
