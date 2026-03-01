#pragma once
#include <expected>
#include <memory>
#include "RSymbol/RNames.h"

namespace Citron {
class MSharedExp;
class RTypeArguments;
class IrExp;
struct TranslationContexts;
using DiagPtr = std::shared_ptr<struct Diag>;

std::expected<MSharedExp*, DiagPtr> TranslateIrExpAndMemberNameToMSharedExp(IrExp* baseIrExp, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContexts& contexts);

} // namespace Citron
