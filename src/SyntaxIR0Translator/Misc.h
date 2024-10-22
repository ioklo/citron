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

class TranslationContext;

RTypeArgumentsPtr MakeTypeArgs(std::vector<STypeExpPtr>& typeArgs, TranslationContext& context);

RExpPtr TryCastRExp(RExpPtr&& exp, const RTypePtr& expectedType, TranslationContext& context); // nothrow
RExpPtr CastRExp(RExpPtr&& exp, const RTypePtr& expectedType, TranslationContext& context);


} // namespace SyntaxIR0Translator
} // namespace Citron
