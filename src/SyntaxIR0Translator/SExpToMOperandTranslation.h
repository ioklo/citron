#pragma once
#include <expected>
#include <memory>
#include "MIR/MRead.h"

namespace Citron {

class SExp;
class RType;
struct TranslationContexts;
using DiagPtr = std::shared_ptr<struct Diag>;

std::expected<MRead, DiagPtr> TranslateSExpToMOperand(SExp* exp, RType* hintType, TranslationContexts& contexts);

} // namespace Citron