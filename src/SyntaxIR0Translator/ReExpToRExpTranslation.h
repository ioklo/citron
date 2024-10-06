#pragma once

#include <memory>

namespace Citron {
class RExp;
using RExpPtr = std::shared_ptr<RExp>;

namespace SyntaxIR0Translator {

class ReExp;

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

RExpPtr TranslateReExpToRExp(ReExp& reExp, const ScopeContextPtr& context, const LoggerPtr& logger);

} // namespace Citron
} // namespace SyntaxIR0Translator