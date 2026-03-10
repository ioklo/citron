#pragma once

#include <memory>
#include <string>
#include <expected>

#include "Logging/Diag.h"
#include "RSymbol/RNames.h"

namespace Citron {

class RTypeArguments;
struct IrExp;
struct TranslationContexts;

std::expected<IrExp*, DiagPtr> TranslateIrExpAndMemberNameToIrExp(IrExp* irExp, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContexts& contexts);

} // namespace Citron