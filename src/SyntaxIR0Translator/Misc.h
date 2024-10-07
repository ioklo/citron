#pragma once

#include <memory>
#include <vector>

namespace Citron {

class RExp;
using RExpPtr = std::shared_ptr<RExp>;

class RType;
using RTypePtr = std::shared_ptr<RType>;

class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

class RTypeArguments;
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

class RTypeFactory;

class STypeExp;
using STypeExpPtr = std::shared_ptr<STypeExp>;

namespace SyntaxIR0Translator {

class ScopeContext;

RTypeArgumentsPtr MakeTypeArgs(std::vector<STypeExpPtr>& typeArgs, ScopeContext& context, RTypeFactory& factory);

RExpPtr TryCastRExp(const RExpPtr& exp, const RTypePtr& expectedType, ScopeContext& context); // nothrow
RExpPtr CastRExp(const RExpPtr& exp, const RTypePtr& expectedType, ScopeContext& context, const LoggerPtr& logger);
RExpPtr MakeRAsExp(const RTypePtr& targetType, const RTypePtr& testType, RExpPtr&& targetExp);

} // namespace SyntaxIR0Translator
} // namespace Citron
