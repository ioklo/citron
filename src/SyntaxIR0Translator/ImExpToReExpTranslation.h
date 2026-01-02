#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"

namespace Citron {

class ReExp;
class ImExp;
struct TranslationContexts;

std::expected<ReExp*, DiagPtr> TranslateImExpToReExp(ImExp* imExp, TranslationContexts& contexts);

} // namespace Citron