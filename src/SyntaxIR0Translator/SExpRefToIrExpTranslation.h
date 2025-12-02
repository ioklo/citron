#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class IrExp;
class TranslationContext;

std::expected<IrExp*, DiagPtr> TranslateSExpRefToIrExp(SExp* exp, TranslationContext& context);

} // namespace Citron
