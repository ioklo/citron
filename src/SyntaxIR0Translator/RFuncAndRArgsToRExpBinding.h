#pragma once

#include <memory>
#include <vector>

#include <IR0/RArgument.h>

namespace Citron {

class RFuncDecl;
using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;
using RExpPtr = std::shared_ptr<class RExp>;
using RLocPtr = std::shared_ptr<class RLoc>;

namespace SyntaxIR0Translator {

RExpPtr BindRFuncAndRArgsToRExp(const std::shared_ptr<RFuncDecl>& decl, const RTypeArgumentsPtr& typeArgs, RLocPtr&& instance, std::vector<RArgument>&& args);

} // namespace SyntaxIR0Translator
} // namespace Citron