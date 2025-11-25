#pragma once
#include <expected>
#include <memory>
#include <optional>
#include "QIR/QArgs.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class MExp_String;

namespace IR0IR1Translator {

class QBodyContext;

std::expected<void, DiagPtr> TranslateMExp_StringToQInsts(MExp_String* exp, std::optional<QArg_StackSlot> destSlot, QBodyContext& bodyContext);

} // namespace IR0IR1Translator
} // namespace Citron