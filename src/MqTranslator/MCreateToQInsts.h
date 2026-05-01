#pragma once
#include <expected>
#include <memory>
#include <optional>
#include "MIR/MCreate.h"
#include "MqCreateTarget.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
struct MqTranslationContexts;

template<typename T>
struct MqEmitState;

std::expected<MqEmitState<void>, DiagPtr> TranslateMCreate_NBCToQInsts(MInitExp* mInitExp, MqCreateTarget createTarget, MqTranslationContexts& contexts);
std::expected<MqEmitState<void>, DiagPtr> TranslateMCreateToQInsts(MCreate& mCreate, MqCreateTarget createTarget, MqTranslationContexts& contexts);

} // namespace Citron
