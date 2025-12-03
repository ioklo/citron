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
std::expected<void, DiagPtr> TranslateMExpToQInsts(MExp* mExp, std::optional<QArg_Slot> oDest, QBodyContext& bodyContext);
} // namespace Citron
