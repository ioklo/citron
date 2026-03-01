#pragma once
#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

class SExp;

class IrExp;
struct TranslationContexts;

std::expected<IrExp*, DiagPtr> TranslateSExpToIrExp(SExp* sExp, TranslationContexts& contexts);

} // namespace Citron