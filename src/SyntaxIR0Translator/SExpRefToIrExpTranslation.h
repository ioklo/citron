#pragma once

#include <memory>

namespace Citron {

class SExp;

class IrExp;
using IrExpPtr = std::shared_ptr<IrExp>;

namespace SyntaxIR0Translator {

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

IrExpPtr TranslateSExpRefToIrExp(SExp& exp, const ScopeContextPtr& context);

} // namespace SyntaxIR0Translator
} // namespace Citron