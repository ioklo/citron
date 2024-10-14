#pragma once

#include <memory>

namespace Citron {

class SExp;

using RExpPtr = std::shared_ptr<class RExp>;
using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

using ScopeContextPtr = std::shared_ptr<class ScopeContext>;

RExpPtr TranslateSExpRefToRExp(SExp& exp, ScopeContext& context, Logger& logger);

} // namespace SyntaxIR0Translator
} // namespace Citron