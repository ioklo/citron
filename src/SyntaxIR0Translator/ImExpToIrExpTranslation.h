#pragma once

#include <memory>
#include <expected> 

#include "Logging/Diag.h"

namespace Citron {

struct ImExp;
class IrExp;
struct TranslationContexts;

std::expected<IrExp*, DiagPtr> TranslateImExpToIrExp(ImExp* imExp, TranslationContexts& contexts);

} // namespace Citron