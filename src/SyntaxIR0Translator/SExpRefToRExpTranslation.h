#pragma once

#include <memory>

namespace Citron {

class SExp;

class RExp;
using RExpPtr = std::shared_ptr<RExp>;

class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

namespace SyntaxIR0Translator {

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

RExpPtr TranslateSExpRefToRExp(SExp& exp, const ScopeContextPtr& context, const LoggerPtr& logger);

} // namespace SyntaxIR0Translator
} // namespace Citron