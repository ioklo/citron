#pragma once
#include <expected>
#include <memory>

#include "QIR/QArgs.h"
#include "MqReadResult.h"
#include "MqLocResult.h"

namespace Citron {

struct Diag;
using DiagPtr = std::shared_ptr<Diag>;

struct MLoc;
class QBlock;
class QFactory;
template<typename T>
struct MqEmitState;

struct MqTranslationContexts;

std::expected<MqEmitState<MqLocResult>, DiagPtr> TranslateMLocToQInsts(MLoc* loc, MqTranslationContexts& contexts);

} // namespace Citron