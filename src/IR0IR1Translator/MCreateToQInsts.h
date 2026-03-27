#pragma once
#include <expected>
#include <memory>
#include <optional>
#include "MIR/MCreate.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
struct QTranslationContexts;

std::expected<void, DiagPtr> TranslateMCreate_NBCToQInsts(MInitExp* mInitExp, std::optional<size_t> o_destSlotIndex, QTranslationContexts& contexts);
std::expected<void, DiagPtr> TranslateMCreateToQInsts(MCreate& mCreate, std::optional<size_t> o_destSlotIndex, QTranslationContexts& contexts);

} // namespace Citron
