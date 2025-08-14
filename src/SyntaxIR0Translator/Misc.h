#pragma once

#include <memory>
#include <vector>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"
#include "IR0/RNames.h"

namespace Citron {

class RTypeArguments;
class NExp;
class RType;

namespace SyntaxIR0Translator {

class TranslationContext;

std::expected<RTypeArguments*, DiagPtr> MakeTypeArgs(std::vector<STypeExp*>& typeArgs, TranslationContext& context);

std::expected<NExp*, DiagPtr> CastNExp(NExp* exp, RType* expectedType, TranslationContext& context);

bool IsVarType(STypeExp* typeExp);

RName_CtorParam MakeBaseCtorParamName(size_t index, RName baseParamName);

} // namespace SyntaxIR0Translator
} // namespace Citron