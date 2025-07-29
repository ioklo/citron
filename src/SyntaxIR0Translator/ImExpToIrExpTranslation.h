#pragma once

#include <memory>
#include <expected> 

#include "Logging/Diag.h"

namespace Citron::SyntaxIR0Translator {

class ImExp;
using ImExpPtr = std::shared_ptr<ImExp>;

class IrExp;
using IrExpPtr = std::shared_ptr<IrExp>;

class TranslationContext;

std::expected<IrExpPtr, DiagPtr> TranslateImExpToIrExp(const ImExpPtr& imExp, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator