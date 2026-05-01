#pragma once

#include <memory>
#include <vector>
#include <expected>

#include "Logging/Diag.h"
#include "MIR/MArgument.h"

namespace Citron {

class RFuncDecl;
class RTypeArguments;
struct MStmt;
struct MLoc;
struct TranslationContexts;

std::expected<MStmt*, DiagPtr> TranslateRFuncAndNArgsToMStmt(RFuncDecl* decl, RTypeArguments* typeArgs, MLoc* instance, std::vector<MArgument>&& args, TranslationContexts& contexts);

} // namespace Citron