#pragma once
#include <memory>

namespace Citron {

using LoggerPtr = std::shared_ptr<class Logger>;
class RTypeFactory;

namespace SyntaxIR0Translator {

using ImExpPtr = std::shared_ptr<class ImExp>;
using IrExpPtr = std::shared_ptr<class IrExp>;


IrExpPtr TranslateImExpToIrExp(const ImExpPtr& imExp, RTypeFactory& factory);

} // namespace SyntaxIR0Translator

} // namespace Citron