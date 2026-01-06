#pragma once
#include <expected>
#include <memory>

#include "QIR/QArgs.h"

namespace Citron {

struct Diag;
using DiagPtr = std::shared_ptr<Diag>;

class MLoc;
class QBlock;
class QFactory;

class QBodyContext;

struct QLocResult_Slot { size_t slotIndex; }; // local var
struct QLocResult_PtrSlot { size_t slotIndex; }; // ptr

using QLocResult = std::variant<QLocResult_Slot, QLocResult_PtrSlot>;

std::expected<QLocResult, DiagPtr> TranslateMLocToQInsts(MLoc* loc, QBodyContext& bodyContext);

} // namespace Citron