#pragma once

#include <memory>

namespace Citron {

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

namespace SyntaxIR0Translator {

class ReExp;
using ReExpPtr = std::shared_ptr<ReExp>;

class ImExp;
using ImExpPtr = std::shared_ptr<ImExp>;


ReExpPtr TranslateImExpToReExp(const ImExpPtr& imExp, const ScopeContextPtr& context, const LoggerPtr& logger);

} // namespace SyntaxIR0Translator

} // namespace Citron