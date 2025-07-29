#pragma once

#include <memory>
#include <vector>
#include <expected>

#include "Syntax/Syntax.h"
#include "Logging/Diag.h"
#include "IR0/NExp.h"

namespace Citron {

namespace SyntaxIR0Translator {

class ImExp;
class TranslationContext;

std::expected<NExpPtr, DiagPtr> TranslateImCallableAndSArgsToNExp(ImExp& imCallable, const SExpPtr& sCallable, const SArgumentsPtr& sArgs, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron