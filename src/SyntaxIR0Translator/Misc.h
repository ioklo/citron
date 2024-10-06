#pragma once

#include <memory>

namespace Citron {

class RExp;
using RExpPtr = std::shared_ptr<RExp>;

class RType;
using RTypePtr = std::shared_ptr<RType>;

class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

namespace SyntaxIR0Translator {

RExpPtr TryCastRExp(const RExpPtr& exp, const RTypePtr& expectedType, const ScopeContextPtr& context); // nothrow
RExpPtr CastRExp(const RExpPtr& exp, const RTypePtr& expectedType, const ScopeContextPtr& context, const LoggerPtr& logger);
RExpPtr MakeRAsExp(const RTypePtr& targetType, const RTypePtr& testType, RExpPtr&& targetExp);

} // namespace SyntaxIR0Translator
} // namespace Citron
