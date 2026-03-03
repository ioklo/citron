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
struct MExp;
class RType;
struct TranslationContexts;
enum class RFuncParameterKind;

std::expected<RFuncParameterKind, DiagPtr> MakeParamKind(std::optional<SParamModifier> o_modifier, bool bRef);
std::expected<RTypeArguments*, DiagPtr> MakeRTypeArgs(std::vector<STypeExp*>& typeArgs, TranslationContexts& contexts);
std::expected<MExp*, DiagPtr> CastMExp(MExp* exp, RType* expectedType, TranslationContexts& contexts);

bool IsVarType(STypeExp* typeExp);

RName_CtorParam MakeBaseCtorParamName(size_t index, RName baseParamName);

template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
std::unexpected<std::shared_ptr<Diag>> Error(TArgs&&... args)
{
    return std::unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
}

} // namespace Citron