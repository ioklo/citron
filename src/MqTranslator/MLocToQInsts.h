#pragma once
#include <expected>
#include <memory>

#include "QIR/QArgs.h"

namespace Citron {

struct Diag;
using DiagPtr = std::shared_ptr<Diag>;

struct MLoc;
class QBlock;
class QFactory;
template<typename T>
struct MqEmitState;

struct MqTranslationContexts;

struct MqLocResult_Slot { size_t slotIndex; }; // local var
struct MqLocResult_Ptr { size_t slotIndex; }; // ptr

using MqLocResult = std::variant<MqLocResult_Slot, MqLocResult_Ptr>;

std::expected<MqEmitState<MqLocResult>, DiagPtr> TranslateMLocToQInsts(MLoc* loc, MqTranslationContexts& contexts);

} // namespace Citron