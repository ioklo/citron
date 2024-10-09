#pragma once

#include <memory>
#include <vector>

#include <IR0/RArgument.h>

namespace Citron {

class RFuncDecl;
class RTypeArguments;
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;
class RExp;
using RExpPtr = std::shared_ptr<RExp>;
class RLoc;
using RLocPtr = std::shared_ptr<RLoc>;

namespace SyntaxIR0Translator {

RExpPtr BindRFuncAndRArgsToRExp(const std::shared_ptr<RFuncDecl>& decl, const RTypeArgumentsPtr& typeArgs, RLocPtr&& instance, std::vector<RArgument>&& args);

} // namespace SyntaxIR0Translator
} // namespace Citron