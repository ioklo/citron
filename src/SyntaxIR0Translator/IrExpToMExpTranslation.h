#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"

namespace Citron {

class MExp;

class IrExp;
class TranslationContext;

std::expected<MExp*, DiagPtr> TranslateIrExpToMExp(IrExp* irExp, TranslationContext& context);

} // namespace Citron