#pragma once
#include <expected>
#include <memory>
#include <optional>
#include "QIR/QArgs.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

class QBodyContext;

std::expected<void, DiagPtr> TranslateMExp_StringToQInsts(MExp_String* exp, std::optional<size_t> destSlot, QBodyContext& bodyContext);
std::expected<void, DiagPtr> TranslateMExp_StringToQInstsWithNewScope(MExp_String* exp, std::optional<size_t> destSlot, QBodyContext& bodyContext);

} // namespace Citron