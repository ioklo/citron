#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class RType;

namespace SyntaxIR0Translator {

class TranslationContext;

class ImExp;
using ImExpPtr = std::shared_ptr<ImExp>;

std::expected<ImExpPtr, DiagPtr> TranslateSExpToImExp(SExp& exp, RType* hintType, TranslationContext& context);

} // namespace SyntaxIR0Translator

} // namespace Citron