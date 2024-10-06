#pragma once

#include <memory>

namespace Citron {

class SExp;

class RExp;
using RExpPtr = std::shared_ptr<RExp>;

namespace SyntaxIR0Translator {

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

RExpPtr TranslateSExpRefToRExp(SExp& exp, const ScopeContextPtr& context);

} // namespace SyntaxIR0Translator
} // namespace Citron