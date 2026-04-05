#pragma once
#include <expected>
#include <memory>
#include <optional>
#include "MIR/MCreate.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
struct QTranslationContexts;

template<typename T>
struct QEmitState;

std::expected<QEmitState<void>, DiagPtr> TranslateMCreate_NBCToQInsts(MInitExp* mInitExp, std::optional<size_t> o_destSlotIndex, QTranslationContexts& contexts);
std::expected<QEmitState<void>, DiagPtr> TranslateMCreateToQInsts(MCreate& mCreate, std::optional<size_t> o_destSlotIndex, QTranslationContexts& contexts);

} // namespace Citron
