#pragma once

#include <memory>
#include <string>
#include <expected>

#include "Logging/Diag.h"
#include "IR0/RNames.h"

namespace Citron {

class RTypeArguments;
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

namespace SyntaxIR0Translator {

class IrExp;
using IrExpPtr = std::shared_ptr<IrExp>;

class TranslationContext;

std::expected<IrExpPtr, DiagPtr> TranslateIrExpAndMemberNameToIrExp(const IrExpPtr& irExp, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, TranslationContext& context);

} // namespace SyntaxIR0Translator 
} // namespace Citron