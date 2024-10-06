#pragma once

#include <memory>

namespace Citron {

class SExp;

namespace SyntaxIR0Translator {

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

class ImExp;
using ImExpPtr = std::shared_ptr<ImExp>;

ImExpPtr TranslateSExpToImExp(SExp& exp, const RTypePtr& hintType, const ScopeContextPtr& context);

} // namespace SyntaxIR0Translator
} // namespace Citron