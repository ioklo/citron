#pragma once

#include <memory>
#include <expected>
#include "ReExp.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
struct ImExp;
struct TranslationContexts;

std::expected<ReExp, DiagPtr> TranslateImExpToReExp(ImExp* imExp, TranslationContexts& contexts);

} // namespace Citron