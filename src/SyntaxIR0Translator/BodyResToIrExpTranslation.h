#pragma once

#include <memory>
#include <expected> 

#include "BodyRes.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class RTypeArguments;
struct ImExp;
struct IrExp;
struct TranslationContexts;

std::expected<IrExp*, DiagPtr> TranslateBaseResAndMemberTypeArgsToIrExp(BodyRes& bodyRes, RTypeArguments* memberTypeArgs, TranslationContexts& contexts);

} // namespace Citron