#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class RType;

class TranslationContext;
class ReExp;

std::expected<ReExp*, DiagPtr> TranslateSExpToReExp(SExp* exp, RType* hintType, TranslationContext& context);

} // namespace Citron