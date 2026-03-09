#pragma once

#include <expected>
#include <memory>
#include "MIR/MCreate.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
struct ReExp;
struct TranslationContexts;

std::expected<MCreate, DiagPtr> TranslateReExpToMCreate(ReExp* reExp, TranslationContexts& contexts);

} // namespace Citron
