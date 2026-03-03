#pragma once
#include <memory>
#include <expected>
#include "MIR/MRead.h"

namespace Citron {

class ReExp;
struct TranslationContexts;
using DiagPtr = std::shared_ptr<struct Diag>;

std::expected<MRead, DiagPtr> TranslateReExpToMOperand(ReExp* reExp, TranslationContexts& contexts);

} // namespace Citron
