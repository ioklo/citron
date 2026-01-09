#pragma once

#include <memory>
#include <vector>
#include <optional>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RNames.h"

namespace Citron {

class RTypeArguments;
class MExp;
class RType;
struct TranslationContexts;
enum class RFuncParameterKind;

RFuncParameterKind MakeParamKind(std::optional<SParamModifier> o_modifier);
std::expected<RTypeArguments*, DiagPtr> MakeRTypeArgs(std::vector<STypeExp*>& typeArgs, TranslationContexts& contexts);
std::expected<MExp*, DiagPtr> CastMExp(MExp* exp, RType* expectedType, TranslationContexts& contexts);

bool IsVarType(STypeExp* typeExp);

RName_CtorParam MakeBaseCtorParamName(size_t index, RName baseParamName);

} // namespace Citron