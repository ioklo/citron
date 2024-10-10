#pragma once

#include <memory>
#include <vector>

namespace Citron {

class SArgument;
using LoggerPtr = std::shared_ptr<class Logger>;
using RExpPtr = std::shared_ptr<class RExp>;

namespace SyntaxIR0Translator {

using ImExpPtr = std::shared_ptr<class ImExp>;
using ScopeContextPtr = std::shared_ptr<class ScopeContext>;

RExpPtr BindImCallableAndSArgsToRExp(ImExpPtr&& imExp, const std::vector<SArgument>& args, const ScopeContextPtr& context, const LoggerPtr& logger);

} // namespace SyntaxIR0Translator
} // namespace Citron