#pragma once
#include <expected>
#include <memory>
#include "MIR/MCreate.h"
#include "MqCreateTarget.h"
#include "MqEmitState.h"

namespace Citron {

struct MqTranslationContexts;
using DiagPtr = std::shared_ptr<struct Diag>;

std::expected<MqEmitState<void>, DiagPtr> TranslateMCreateToQInsts(MCreate& create, MqCreateTarget createTarget, MqTranslationContexts& contexts);

} // namespace Citron
