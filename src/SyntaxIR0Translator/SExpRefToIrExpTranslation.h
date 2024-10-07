#pragma once

#include <memory>

namespace Citron {

class SExp;

namespace SyntaxIR0Translator {

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

class IrExp;
using IrExpPtr = std::shared_ptr<IrExp>;

IrExpPtr TranslateSExpRefToIrExp(SExp& exp, const ScopeContextPtr& context);

} // namespace SyntaxIR0Translator
} // namespace Citron