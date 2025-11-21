#pragma once
#include <expected>
#include <memory>

#include "QIR/QValues.h"

namespace Citron {

struct Diag;
using DiagPtr = std::shared_ptr<Diag>;

class MLoc;
class QBlock;
class QFactory;

namespace IR0IR1Translator {
class QBodyContext;

std::expected<QArg_Register, DiagPtr> TranslateMLocToQInsts(MLoc* loc, QBodyContext& bodyContext);

} // namespace IR0IR1Translator
} // namespace Citron