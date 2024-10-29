#pragma once

#include <memory>
#include <vector>

#include <IR0/NArgument.h>

namespace Citron {

class RFuncDecl;
using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;
using NExpPtr = std::shared_ptr<class NExp>;
using NLocPtr = std::shared_ptr<class NLoc>;

namespace SyntaxIR0Translator {

NExpPtr TranslateRFuncAndNArgsToNExp(const std::shared_ptr<RFuncDecl>& decl, const RTypeArgumentsPtr& typeArgs, NLocPtr&& instance, std::vector<NArgument>&& args);

} // namespace SyntaxIR0Translator
} // namespace Citron