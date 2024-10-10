#pragma once

#include <memory>

namespace Citron {

using RExpPtr = std::shared_ptr<class RExp>;
using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

class IrExp;
using ScopeContextPtr = std::shared_ptr<class ScopeContext>;


RExpPtr TranslateIrExpToRExp(IrExp& irExp, const ScopeContextPtr& context, const LoggerPtr& logger);

} // SyntaxIR0Translator 

} // Citron