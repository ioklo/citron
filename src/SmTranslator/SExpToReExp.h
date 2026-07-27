#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"
#include "ReExp.h"

namespace Citron {

class RType;
struct SmTranslationContexts;

std::expected<ReExp, DiagPtr> TranslateSExpToReExp(SExp* exp, RType* hintType, SmTranslationContexts& contexts);

} // namespace Citron