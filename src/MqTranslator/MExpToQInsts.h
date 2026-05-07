#pragma once
#include <memory>
#include <expected>

#include "MqEmitState.h"
#include "MqReadResult.h"
#include "MqCreateTarget.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

struct MExp;
struct MqTranslationContexts;

// MExp는 ReadResult
std::expected<MqEmitState<MqReadResult>, DiagPtr> TranslateMExpToQInstsForRead(MExp* mExp, MqTranslationContexts& contexts);
std::expected<MqEmitState<void>, DiagPtr> TranslateMExpToQInstsForCreate(MExp* mExp, MqCreateTarget createTarget, MqTranslationContexts& contexts);

} // namespace Citron
