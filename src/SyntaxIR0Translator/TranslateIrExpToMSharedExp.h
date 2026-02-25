#pragma once
#include <memory>
#include <expected>

namespace Citron {

struct TranslationContexts;
class MSharedExp;
class IrExp;

using DiagPtr = std::shared_ptr<struct Diag>;

std::expected<MSharedExp*, DiagPtr> TranslateIrExpToMSharedExp(IrExp* irExp, TranslationContexts& contexts);


} // namespace Citron