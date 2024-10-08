#pragma once

#include <memory>
#include <string>

namespace Citron {

class RTypeArguments;
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;
class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

namespace SyntaxIR0Translator {

class ImExp;
using ImExpPtr = std::shared_ptr<ImExp>;
class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

ImExpPtr BindImExpAndMemberNameToImExp(ImExp& imExp, const std::string& name, const RTypeArgumentsPtr& typeArgs, const ScopeContextPtr& context, const LoggerPtr& logger);

} // namespace SyntaxIR0Translator
} // namespace Citron