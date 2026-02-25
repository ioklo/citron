#pragma once
#include <expected>
#include <memory>
#include "RSymbol/RNames.h"

using DiagPtr = std::shared_ptr<struct Diag>;

namespace Citron {
class MSharedExp;
class RTypeArguments;
class IrExp;
struct TranslationContexts;

std::expected<MSharedExp*, DiagPtr> TranslateIrExpAndMemberNameToMSharedExp(IrExp* irExp, const RName& name, RTypeArguments* typeArgsExceptOuter, TranslationContexts& contexts);

} // namespace Citron
