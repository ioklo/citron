#pragma once

#include <memory>

namespace Citron {

class RTypeFactory;

using SExpPtr = std::shared_ptr<class SExp>;
using RLocPtr = std::shared_ptr<class RLoc>;
using RTypePtr = std::shared_ptr<class RType>;
using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

using ScopeContextPtr = std::shared_ptr<class ScopeContext>;

class IDesignatedErrorLogger;

RLocPtr TranslateSExpToRLoc(SExp& sExp, const RTypePtr& hintType, bool bWrapExpAsLoc, IDesignatedErrorLogger* notLocationLogger, ScopeContext& context, Logger& logger, RTypeFactory& factory);

} // namespace SyntaxIR0Translator 

} // namespace Citron