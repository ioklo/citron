#pragma once
#include <memory>

namespace Citron {

class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

class RTypeFactory;

namespace SyntaxIR0Translator {

class ImExp;
using ImExpPtr = std::shared_ptr<ImExp>;

class IrExp;
using IrExpPtr = std::shared_ptr<IrExp>;


IrExpPtr TranslateImExpToIrExp(const ImExpPtr& imExp, RTypeFactory& factory);

} // namespace SyntaxIR0Translator

} // namespace Citron