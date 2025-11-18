#pragma once

#include <memory>
#include <vector>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RNames.h"

namespace Citron {

class RTypeArguments;
class MExp;
class RType;

namespace SyntaxIR0Translator {

class TranslationContext;

std::expected<RTypeArguments*, DiagPtr> MakeRTypeArgs(std::vector<STypeExp*>& typeArgs, TranslationContext& context);

std::expected<MExp*, DiagPtr> CastMExp(MExp* exp, RType* expectedType, TranslationContext& context);

bool IsVarType(STypeExp* typeExp);

RName_CtorParam MakeBaseCtorParamName(size_t index, RName baseParamName);

} // namespace SyntaxIR0Translator
} // namespace Citron