#pragma once

#include <memory>
#include <vector>

namespace Citron {

using RExpPtr = std::shared_ptr<class RExp>;
using RTypePtr = std::shared_ptr<class RType>;
using LoggerPtr = std::shared_ptr<class Logger>;
using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;
class RTypeFactory;
using STypeExpPtr = std::shared_ptr<class STypeExp>;

namespace SyntaxIR0Translator {

class ScopeContext;

RTypeArgumentsPtr MakeTypeArgs(std::vector<STypeExpPtr>& typeArgs, ScopeContext& context, RTypeFactory& factory);

RExpPtr TryCastRExp(RExpPtr&& exp, const RTypePtr& expectedType, ScopeContext& context); // nothrow
RExpPtr CastRExp(RExpPtr&& exp, const RTypePtr& expectedType, ScopeContext& context, Logger& logger);
RExpPtr MakeRExp_As(RExpPtr&& targetExp, const RTypePtr& testType, RTypeFactory& factory);

} // namespace SyntaxIR0Translator
} // namespace Citron
