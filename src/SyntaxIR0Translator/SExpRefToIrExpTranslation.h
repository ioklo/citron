#pragma once

#include <memory>

namespace Citron {

class SExp;

namespace SyntaxIR0Translator {

using ScopeContextPtr = std::shared_ptr<class ScopeContext>;
using IrExpPtr = std::shared_ptr<class IrExp>;

IrExpPtr TranslateSExpRefToIrExp(SExp& exp, const ScopeContextPtr& context);

} // namespace SyntaxIR0Translator
} // namespace Citron