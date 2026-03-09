#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class RType;
struct ReExp;
struct TranslationContexts;

std::expected<ReExp*, DiagPtr> TranslateSExpToReExp(SExp* exp, RType* hintType, TranslationContexts& contexts);

} // namespace Citron