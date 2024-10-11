#pragma once
#include <memory>

namespace Citron {

using LoggerPtr = std::shared_ptr<class Logger>;
class RTypeFactory;

namespace SyntaxIR0Translator {

using ImExpPtr = std::shared_ptr<class ImExp>;
using IrExpPtr = std::shared_ptr<class IrExp>;

class ScopeContext;

IrExpPtr TranslateImExpToIrExp(const ImExpPtr& imExp, ScopeContext& context, RTypeFactory& factory);

} // namespace SyntaxIR0Translator

} // namespace Citron