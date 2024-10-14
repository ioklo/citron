#pragma once

#include <memory>

namespace Citron {

using ScopeContextPtr = std::shared_ptr<class ScopeContext>;
using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

using ReExpPtr = std::shared_ptr<class ReExp>;
class ImExp;

ReExpPtr TranslateImExpToReExp(ImExp& imExp, ScopeContext& context, Logger& logger);

} // namespace SyntaxIR0Translator

} // namespace Citron