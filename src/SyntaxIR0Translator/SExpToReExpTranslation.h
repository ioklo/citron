#pragma once

#include <memory>

namespace Citron {

class SExp;

class RType;
using RTypePtr = std::shared_ptr<RType>;

namespace SyntaxIR0Translator {

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

class ReExp;
using ReExpPtr = std::shared_ptr<ReExp>;

ReExpPtr TranslateSExpToReExp(SExp& exp, const RTypePtr& hintType, const ScopeContextPtr& context);

} // namespace SyntaxIR0Translator
} // namespace Citron