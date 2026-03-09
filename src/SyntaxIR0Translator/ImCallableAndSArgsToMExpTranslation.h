#pragma once

#include <memory>
#include <vector>
#include <expected>

#include "Syntax/Syntax.h"
#include "Logging/Diag.h"
#include "MIR/MExp.h"

namespace Citron {

struct ImExp;
struct TranslationContexts;

std::expected<MExp*, DiagPtr> TranslateImCallableAndSArgsToMExp(ImExp* imCallable, SExp* sCallable, SArguments* sArgs, TranslationContexts& contexts);

} // namespace Citron