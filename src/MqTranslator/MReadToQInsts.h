#pragma once
#include <memory>
#include <expected>
#include "MIR/MRead.h"
#include "MqEmitState.h"
#include "MqReadResult.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
struct MqTranslationContexts;

std::expected<MqEmitState<MqReadResult>, DiagPtr> TranslateMReadToQInsts(MRead& mRead, MqTranslationContexts& contexts);

} // namespace Citron
