#pragma once

#include <memory>
#include <string>

namespace Citron {

class RExp;
using RExpPtr = std::shared_ptr<RExp>;

class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

class RTypeArguments;
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

namespace SyntaxIR0Translator {

class IrExp;
using IrExpPtr = std::shared_ptr<IrExp>;

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

IrExpPtr BindIrExpAndMemberNameToIrExp(IrExp& irExp, const std::string& name, RTypeArgumentsPtr&& typeArgs, const ScopeContextPtr& context);

} // SyntaxIR0Translator 

} // Citron