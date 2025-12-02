#pragma once

#include <memory>
#include <string>
#include <expected>

#include "Logging/Diag.h"
#include "RSymbol/RNames.h"

namespace Citron {

class RTypeArguments;

class IrExp;
class TranslationContext;

std::expected<IrExp*, DiagPtr> TranslateIrExpAndMemberNameToIrExp(IrExp* irExp, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContext& context);

} // namespace Citron