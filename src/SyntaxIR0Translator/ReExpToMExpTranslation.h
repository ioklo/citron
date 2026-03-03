#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"

namespace Citron {

struct MExp;
class ReExp;
struct TranslationContexts;

std::expected<MExp*, DiagPtr> TranslateReExpToMExp(ReExp* reExp, TranslationContexts& contexts);

} // namespace Citron
