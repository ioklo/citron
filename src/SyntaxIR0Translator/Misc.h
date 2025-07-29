#pragma once

#include <memory>
#include <vector>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"
#include "IR0/RNames.h"

namespace Citron {

class RTypeArguments;
using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;
class NExp;
using NExpPtr = std::shared_ptr<NExp>;
class RType;
using RTypePtr = std::shared_ptr<RType>;

namespace SyntaxIR0Translator {

class TranslationContext;

std::expected<RTypeArgumentsPtr, DiagPtr> MakeTypeArgs(std::vector<STypeExpPtr>& typeArgs, TranslationContext& context);

std::expected<NExpPtr, DiagPtr> CastNExp(NExpPtr&& exp, const RTypePtr& expectedType, TranslationContext& context);
std::expected<NExpPtr, DiagPtr> CastNExp(const NExpPtr& exp, const RTypePtr& expectedType, TranslationContext& context);

bool IsVarType(STypeExp& typeExp);

RName_CtorParam MakeBaseCtorParamName(size_t index, RName baseParamName);

} // namespace SyntaxIR0Translator
} // namespace Citron