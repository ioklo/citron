#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"

namespace Citron {

class MExp;

class TranslationContext;
class ReExp;

std::expected<MExp*, DiagPtr> TranslateReExpToMExp(ReExp* reExp, TranslationContext& context);

} // namespace Citron
