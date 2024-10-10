#pragma once

#include <memory>
#include <vector>

namespace Citron {

class RExp;
using RExpPtr = std::shared_ptr<RExp>;
class SArgument;

class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

namespace SyntaxIR0Translator {

class ImExp;
using ImExpPtr = std::shared_ptr<ImExp>;

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

RExpPtr BindImCallableAndSArgsToRExp(ImExpPtr&& imExp, const std::vector<SArgument>& args, const ScopeContextPtr& context, const LoggerPtr& logger);

} // namespace SyntaxIR0Translator
} // namespace Citron