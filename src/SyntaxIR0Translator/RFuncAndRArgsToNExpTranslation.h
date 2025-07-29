#pragma once

#include <memory>
#include <vector>
#include <expected>

#include "Logging/Diag.h"
#include "IR0/NArgument.h"

namespace Citron {

class RFuncDecl;
class RTypeArguments;
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;
class NExp;
using NExpPtr = std::shared_ptr<NExp>;
class NLoc;
using NLocPtr = std::shared_ptr<NLoc>;

namespace SyntaxIR0Translator {

std::expected<NExpPtr, DiagPtr> TranslateRFuncAndNArgsToNExp(const std::shared_ptr<RFuncDecl>& decl, const RTypeArgumentsPtr& typeArgs, NLocPtr&& instance, std::vector<NArgument>&& args);

} // namespace SyntaxIR0Translator
} // namespace Citron