#pragma once

#include <memory>
#include <expected>

#include "Syntax/Syntax.h"
#include "ReExp.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class RType;
struct SmTranslationContexts;

std::expected<ReExp, DiagPtr> TranslateSExpToReExp(SExp* exp, RType* hintType, SmTranslationContexts& contexts);

} // namespace Citron