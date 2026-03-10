#pragma once

#include <memory>
#include <expected>
#include "ReExp.h"

#include "Logging/Diag.h"

namespace Citron {

struct ImExp;
struct TranslationContexts;

std::expected<ReExp, DiagPtr> TranslateImExpToReExp(ImExp* imExp, TranslationContexts& contexts);

} // namespace Citron