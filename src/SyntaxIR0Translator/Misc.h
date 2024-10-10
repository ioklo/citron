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

RExpPtr TryCastRExp(const RExpPtr& exp, const RTypePtr& expectedType, ScopeContext& context); // nothrow
RExpPtr CastRExp(const RExpPtr& exp, const RTypePtr& expectedType, ScopeContext& context, const LoggerPtr& logger);
RExpPtr MakeRAsExp(const RTypePtr& targetType, const RTypePtr& testType, RExpPtr&& targetExp);

} // namespace SyntaxIR0Translator
} // namespace Citron
