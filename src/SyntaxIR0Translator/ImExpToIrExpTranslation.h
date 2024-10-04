#pragma once
#include <memory>

namespace Citron {

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

namespace SyntaxIR0Translator {

class ImExp;
using ImExpPtr = std::shared_ptr<ImExp>;

class IrExp;
using IrExpPtr = std::shared_ptr<IrExp>;

IrExpPtr TranslateImExpToIrExp(const ImExpPtr& imExp);

} // namespace SyntaxIR0Translator

} // namespace Citron