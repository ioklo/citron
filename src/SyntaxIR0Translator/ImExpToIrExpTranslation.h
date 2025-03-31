#pragma once
import <memory>;

namespace Citron {

using LoggerPtr = std::shared_ptr<class Logger>;
class RTypeFactory;

namespace SyntaxIR0Translator {

using ImExpPtr = std::shared_ptr<class ImExp>;
using IrExpPtr = std::shared_ptr<class IrExp>;

class TranslationContext;

IrExpPtr TranslateImExpToIrExp(const ImExpPtr& imExp, TranslationContext& context, RTypeFactory& factory);

} // namespace SyntaxIR0Translator

} // namespace Citron