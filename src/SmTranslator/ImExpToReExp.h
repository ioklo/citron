#pragma once

#include <memory>
#include <expected>
#include "ReExp.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
struct ImExp;
struct SmTranslationContexts;

std::expected<ReExp, DiagPtr> TranslateImExpToReExp(ImExp* imExp, SmTranslationContexts& contexts);

} // namespace Citron