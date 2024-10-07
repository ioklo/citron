#pragma once

#include <memory>

namespace Citron {

class RExp;
using RExpPtr = std::shared_ptr<RExp>;

class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

namespace SyntaxIR0Translator {

class IrExp;
class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;


RExpPtr TranslateIrExpToRExp(IrExp& irExp, const ScopeContextPtr& context, const LoggerPtr& logger);

} // SyntaxIR0Translator 

} // Citron