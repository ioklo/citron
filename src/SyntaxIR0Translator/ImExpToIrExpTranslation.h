#pragma once

#include <memory>
#include <expected> 

#include "Logging/Diag.h"

namespace Citron {

class ImExp;
class IrExp;
struct TranslationContexts;

std::expected<IrExp*, DiagPtr> TranslateImExpToIrExp(ImExp* imExp, TranslationContexts& contexts);

} // namespace Citron