#pragma once

#include <memory>
#include <expected> 

#include "Logging/Diag.h"

namespace Citron::SyntaxIR0Translator {

class ImExp;
class TranslationContext;

std::expected<IrExp*, DiagPtr> TranslateImExpToIrExp(ImExp* imExp, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator