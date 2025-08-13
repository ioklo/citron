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

std::expected<NExp*, DiagPtr> TranslateRFuncAndNArgsToNExp(RFuncDecl* decl, RTypeArguments* typeArgs, NLoc* instance, std::vector<NArgument>&& args);

} // namespace SyntaxIR0Translator
} // namespace Citron