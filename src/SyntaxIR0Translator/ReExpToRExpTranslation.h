#pragma once

#include <memory>

namespace Citron {
class RExp;
using RExpPtr = std::shared_ptr<RExp>;

class RTypeFactory;

class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

namespace SyntaxIR0Translator {

class ReExp;

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

RExpPtr TranslateReExpToRExp(ReExp& reExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);

} // namespace Citron
} // namespace SyntaxIR0Translator