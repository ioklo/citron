#pragma once

#include <memory>
#include <vector>
#include <expected>

#include "Syntax/Syntax.h"
#include "Logging/Diag.h"
#include "MIR/MExp.h"

namespace Citron {

namespace SyntaxIR0Translator {

class ImExp;
class TranslationContext;

std::expected<MExp*, DiagPtr> TranslateImCallableAndSArgsToMExp(ImExp* imCallable, SExp* sCallable, SArguments* sArgs, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron