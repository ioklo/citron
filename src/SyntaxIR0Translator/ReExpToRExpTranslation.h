#pragma once

#include <memory>

namespace Citron {
using RExpPtr = std::shared_ptr<class RExp>;

class RTypeFactory;

using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

class ReExp;

using ScopeContextPtr = std::shared_ptr<class ScopeContext>;

RExpPtr TranslateReExpToRExp(ReExp& reExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);

} // namespace Citron
} // namespace SyntaxIR0Translator