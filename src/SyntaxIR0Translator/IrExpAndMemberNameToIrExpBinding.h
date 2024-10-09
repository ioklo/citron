#pragma once

#include <memory>
#include <string>

#include <IR0/RNames.h>

namespace Citron {

class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

class RTypeArguments;
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

class RTypeFactory;

namespace SyntaxIR0Translator {

class IrExp;
using IrExpPtr = std::shared_ptr<IrExp>;

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

IrExpPtr BindIrExpAndMemberNameToIrExp(const IrExpPtr& irExp, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);


} // SyntaxIR0Translator 

} // Citron