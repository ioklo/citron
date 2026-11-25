#pragma once

#include <memory>
#include <vector>
#include <expected>

#include "MIR/MArgument.h"
#include "RSymbol/RFuncDecl.h"

namespace Citron {

class RTypeArguments;
struct MStmt;
struct MLoc;
struct SmTranslationContexts;
using DiagPtr = std::shared_ptr<struct Diag>;

std::expected<MStmt*, DiagPtr> TranslateRFuncAndNArgsToMStmt(RFuncDecl* decl, RTypeArguments* typeArgs, MLoc* instance, std::vector<MArgument>&& args, SmTranslationContexts& contexts);

} // namespace Citron