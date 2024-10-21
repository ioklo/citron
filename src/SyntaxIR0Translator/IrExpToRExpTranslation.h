#pragma once

#include <memory>

namespace Citron {

using RExpPtr = std::shared_ptr<class RExp>;
using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

class IrExp;
class TranslationContext;

RExpPtr TranslateIrExpToRExp(IrExp& irExp, TranslationContext& context);

} // SyntaxIR0Translator 

} // Citron