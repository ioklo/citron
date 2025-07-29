#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class RType;
using RTypePtr = std::shared_ptr<RType>;

namespace SyntaxIR0Translator {

class TranslationContext;

class ImExp;
using ImExpPtr = std::shared_ptr<ImExp>;

std::expected<ImExpPtr, DiagPtr> TranslateSExpToImExp(SExp& exp, const RTypePtr& hintType, TranslationContext& context);

} // namespace SyntaxIR0Translator

} // namespace Citron