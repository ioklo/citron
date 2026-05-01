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

struct QLocResult_Slot { size_t slotIndex; }; // local var
struct QLocResult_Ptr { size_t slotIndex; }; // ptr

using QLocResult = std::variant<QLocResult_Slot, QLocResult_Ptr>;

std::expected<MqEmitState<QLocResult>, DiagPtr> TranslateMLocToQInsts(MLoc* loc, MqTranslationContexts& contexts);

} // namespace Citron