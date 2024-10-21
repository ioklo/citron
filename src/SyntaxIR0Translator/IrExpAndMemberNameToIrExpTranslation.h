#pragma once

#include <memory>
#include <string>

#include <IR0/RNames.h>

namespace Citron {

using LoggerPtr = std::shared_ptr<class Logger>;
using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;
class RTypeFactory;

namespace SyntaxIR0Translator {

using IrExpPtr = std::shared_ptr<class IrExp>;
class TranslationContext;

IrExpPtr TranslateIrExpAndMemberNameToIrExp(const IrExpPtr& irExp, const RName& name, const RTypeArgumentsPtr& typeArgsExceptOuter, TranslationContext& context);


} // SyntaxIR0Translator 

} // Citron