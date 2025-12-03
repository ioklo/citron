#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class MExp;

class TranslationContext;

std::expected<MExp*, DiagPtr> TranslateSExpRefToMExp(SExp* exp, TranslationContext& context);

} // namespace Citron
