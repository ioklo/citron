#pragma once

#include <memory>

namespace Citron {

class SExp;
class RTypeFactory;
using RTypePtr = std::shared_ptr<class RType>;
using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

using ScopeContextPtr = std::shared_ptr<class ScopeContext>;
using ReExpPtr = std::shared_ptr<class ReExp>;

ReExpPtr TranslateSExpToReExp(SExp& exp, const RTypePtr& hintType, ScopeContext& context, Logger& logger, RTypeFactory& factory);

} // namespace SyntaxIR0Translator
} // namespace Citron