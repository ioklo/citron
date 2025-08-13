#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class RType;

namespace SyntaxIR0Translator {

class TranslationContext;
class ReExp;
using ReExpPtr = std::shared_ptr<ReExp>;

std::expected<ReExpPtr, DiagPtr> TranslateSExpToReExp(SExp& exp, RType* hintType, TranslationContext& context);

} // namespace SyntaxIR0Translator

} // namespace Citron