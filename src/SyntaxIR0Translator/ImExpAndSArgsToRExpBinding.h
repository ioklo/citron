#pragma once
#include <memory>
#include <vector>

namespace Citron {

class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

class RExp;
using RExpPtr = std::shared_ptr<RExp>;

class SArgument;

namespace SyntaxIR0Translator {

class ImExp;
using ImExpPtr = std::shared_ptr<ImExp>;

class IrExp;
using IrExpPtr = std::shared_ptr<IrExp>;

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

RExpPtr BindImExpAndSArgsToRExp(ImExp& imExp, std::vector<SArgument>& sArgs, const ScopeContextPtr& context, const LoggerPtr& logger);

} // namespace SyntaxIR0Translator

} // namespace Citron