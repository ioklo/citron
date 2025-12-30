#pragma once
#include <expected>
#include <memory>
#include <optional>

#include "QIR/QArgs.h"

namespace Citron {

class MExp;
class QBlock;
class QFactory;
struct Diag;
using DiagPtr = std::shared_ptr<Diag>;

class QBodyContext;

// mExp를 여러개의 qInst로 번역해서 block에 집어넣는다.
std::expected<void, DiagPtr> TranslateMExpToQInsts(MExp* mExp, std::optional<size_t> oDestSlotIndex, QBodyContext& bodyContext);
std::expected<void, DiagPtr> TranslateMExpToQInstsWithNewScope(MExp* mExp, std::optional<size_t> oDestSlotIndex, QBodyContext& bodyContext);
} // namespace Citron
