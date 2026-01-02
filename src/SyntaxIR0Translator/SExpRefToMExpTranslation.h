#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class MExp;
struct TranslationContexts;

std::expected<MExp*, DiagPtr> TranslateSExpRefToMExp(SExp* exp, TranslationContexts& contexts);

} // namespace Citron
