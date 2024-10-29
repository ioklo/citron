#pragma once

#include <memory>

namespace Citron {

using NExpPtr = std::shared_ptr<class NExp>;
using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

class IrExp;
class TranslationContext;

NExpPtr TranslateIrExpToNExp(IrExp& irExp, TranslationContext& context);

} // SyntaxIR0Translator 

} // Citron