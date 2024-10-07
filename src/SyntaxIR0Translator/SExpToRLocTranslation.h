#pragma once

#include <memory>

namespace Citron {

class SExp;
using SExpPtr = std::shared_ptr<SExp>;

class RLoc;
using RLocPtr = std::shared_ptr<RLoc>;

class RType;
using RTypePtr = std::shared_ptr<RType>;

class RTypeFactory;

class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

namespace SyntaxIR0Translator {

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

class INotLocationErrorLogger;

RLocPtr TranslateSExpToRLoc(SExp& sExp, const RTypePtr& hintType, bool bWrapExpAsLoc, INotLocationErrorLogger* notLocationLogger, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);

} // namespace SyntaxIR0Translator 

} // namespace Citron