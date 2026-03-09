#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"

namespace Citron {

struct ReExp;
struct ImExp;
struct TranslationContexts;

std::expected<ReExp*, DiagPtr> TranslateImExpToReExp(ImExp* imExp, TranslationContexts& contexts);

} // namespace Citron