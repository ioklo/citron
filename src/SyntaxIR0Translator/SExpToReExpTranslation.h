#pragma once

#include <memory>

namespace Citron {

class SExp;

class RType;
using RTypePtr = std::shared_ptr<RType>;

class RTypeFactory;

class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

namespace SyntaxIR0Translator {

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

class ReExp;
using ReExpPtr = std::shared_ptr<ReExp>;

ReExpPtr TranslateSExpToReExp(SExp& exp, const RTypePtr& hintType, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);

} // namespace SyntaxIR0Translator
} // namespace Citron