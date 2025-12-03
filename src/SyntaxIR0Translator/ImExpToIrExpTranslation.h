#pragma once

#include <memory>
#include <expected> 

#include "Logging/Diag.h"

namespace Citron {

class ImExp;
class IrExp;
class TranslationContext;

std::expected<IrExp*, DiagPtr> TranslateImExpToIrExp(ImExp* imExp, TranslationContext& context);

} // namespace Citron