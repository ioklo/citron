#pragma once

#include <memory>
#include <vector>

namespace Citron {

using SExpPtr = std::shared_ptr<class SExp>;
using SArgumentsPtr = std::shared_ptr<class SArguments>;
using LoggerPtr = std::shared_ptr<class Logger>;
using RExpPtr = std::shared_ptr<class RExp>;

class RTypeFactory;

namespace SyntaxIR0Translator {

class ImExp;
using ScopeContextPtr = std::shared_ptr<class ScopeContext>;

RExpPtr TranslateImCallableAndSArgsToRExp(ImExp& imCallable, const SExpPtr& sCallable, const SArgumentsPtr& sArgs, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);

} // namespace SyntaxIR0Translator
} // namespace Citron