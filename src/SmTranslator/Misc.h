#pragma once

#include <memory>
#include <vector>
#include <optional>
#include <expected>

#include "Infra/Ptr.h"
#include "Logging/Diag.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RNames.h"

namespace Citron {

class RTypeArguments;
struct MExp;
class RType;
struct SmTranslationContexts;
enum class RFuncParameterKind;

std::expected<RFuncParameterKind, DiagPtr> MakeParamKind(std::optional<SParamModifier> o_modifier, bool bRef);
// TODO: [47] CastMExp의 리턴값 수정, NBC의 암시적 Cast구현하기
// std::expected<MExp*, DiagPtr> CastMExp(MExp* exp, RType* expectedType, SmTranslationContexts& contexts);

bool IsVarType(STypeExp* typeExp);

} // namespace Citron