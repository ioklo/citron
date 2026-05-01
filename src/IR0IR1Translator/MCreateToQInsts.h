#pragma once
#include <expected>
#include <memory>
#include <optional>
#include "MIR/MCreate.h"
#include "MqCreateTarget.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
struct QTranslationContexts;

template<typename T>
struct QEmitState;

std::expected<QEmitState<void>, DiagPtr> TranslateMCreate_NBCToQInsts(MInitExp* mInitExp, MqCreateTarget createTarget, QTranslationContexts& contexts);
std::expected<QEmitState<void>, DiagPtr> TranslateMCreateToQInsts(MCreate& mCreate, MqCreateTarget createTarget, QTranslationContexts& contexts);

} // namespace Citron
