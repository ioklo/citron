#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class RType;
struct ImExp;
struct TranslationContexts;

std::expected<ImExp*, DiagPtr> TranslateSExpToImExp(SExp* exp, RType* hintType, TranslationContexts& contexts);

} // namespace Citron