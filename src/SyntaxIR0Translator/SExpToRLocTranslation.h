#pragma once

#include <memory>

namespace Citron {

class SExp;
using SExpPtr = std::shared_ptr<SExp>;

class RLoc;
using RLocPtr = std::shared_ptr<RLoc>;

class RType;
using RTypePtr = std::shared_ptr<RType>;

namespace SyntaxIR0Translator {

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

class INotLocationErrorLogger;

RLocPtr TranslateSExpToRLoc(SExpPtr exp, const ScopeContextPtr& context, const RTypePtr& hintType, bool bWrapExpAsLoc, INotLocationErrorLogger* notLocationLogger);

} // namespace SyntaxIR0Translator 

} // namespace Citron