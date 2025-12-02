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

class TranslationContext;

std::expected<MExp*, DiagPtr> TranslateRFuncAndNArgsToMExp(RFuncDecl* decl, RTypeArguments* typeArgs, MLoc* instance, std::vector<MArgument>&& args, TranslationContext& context);

} // namespace Citron