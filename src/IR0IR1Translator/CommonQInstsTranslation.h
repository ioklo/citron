#pragma once
#include <expected>
#include <memory>
#include <optional>
#include "QIR/QArgs.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class MExp_String;

class QBodyContext;

std::expected<void, DiagPtr> TranslateMExp_StringToQInsts(MExp_String* exp, std::optional<QArg_Slot> destSlot, QBodyContext& bodyContext);

} // namespace Citron