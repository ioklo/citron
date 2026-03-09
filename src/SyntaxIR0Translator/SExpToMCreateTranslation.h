#pragma once
#include <expected>
#include <memory>
#include "MIR/MCreate.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class SExp;
struct TranslationContexts;

std::expected<MCreate, DiagPtr> TranslateSExpToMCreate(SExp* sExp, RType* hintType, TranslationContexts& contexts);

} // namespace Citron
