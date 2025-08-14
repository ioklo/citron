#pragma once

#include <memory>
#include <vector>
#include <expected>

#include "Logging/Diag.h"
#include "IR0/NArgument.h"

namespace Citron {

class RFuncDecl;
class RTypeArguments;
class NExp;
class NLoc;

namespace SyntaxIR0Translator {

class TranslationContext;

std::expected<NExp*, DiagPtr> TranslateRFuncAndNArgsToNExp(RFuncDecl* decl, RTypeArguments* typeArgs, NLoc* instance, std::vector<NArgument>&& args, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron