#pragma once

#include <memory>
#include <string>

namespace Citron {

using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;
using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

using ImExpPtr = std::shared_ptr<class ImExp>;
using ScopeContextPtr = std::shared_ptr<class ScopeContext>;

ImExpPtr BindImExpAndMemberNameToImExp(ImExp& imExp, const std::string& name, const RTypeArgumentsPtr& typeArgs, const ScopeContextPtr& context, const LoggerPtr& logger);

} // namespace SyntaxIR0Translator
} // namespace Citron