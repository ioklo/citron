#pragma once

#include <memory>
#include <vector>
#include <expected>

#include "Logging/Diag.h"
#include "MIR/MArgument.h"

namespace Citron {

class RFuncDecl;
class RTypeArguments;
class MExp;
class MLoc;

namespace SyntaxIR0Translator {

class TranslationContext;

std::expected<MExp*, DiagPtr> TranslateRFuncAndNArgsToMExp(RFuncDecl* decl, RTypeArguments* typeArgs, MLoc* instance, std::vector<MArgument>&& args, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron