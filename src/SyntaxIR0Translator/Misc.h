#pragma once

#include <memory>
#include <vector>

namespace Citron {

using NExpPtr = std::shared_ptr<class NExp>;
using RTypePtr = std::shared_ptr<class RType>;
using LoggerPtr = std::shared_ptr<class Logger>;
using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;
class RTypeFactory;
using STypeExpPtr = std::shared_ptr<class STypeExp>;

namespace SyntaxIR0Translator {

class TranslationContext;

RTypeArgumentsPtr MakeTypeArgs(std::vector<STypeExpPtr>& typeArgs, TranslationContext& context);

NExpPtr TryCastRExp(NExpPtr&& exp, const RTypePtr& expectedType, TranslationContext& context); // nothrow
NExpPtr CastNExp(NExpPtr&& exp, const RTypePtr& expectedType, TranslationContext& context);

bool IsVarType(STypeExp& typeExp);

RName_CtorParam MakeBaseCtorParamName(int index, RName baseParamName);


} // namespace SyntaxIR0Translator
} // namespace Citron
