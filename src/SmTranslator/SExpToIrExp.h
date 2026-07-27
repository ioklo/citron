#pragma once
#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

class SExp;

struct IrExp;
struct SmTranslationContexts;

std::expected<IrExp*, DiagPtr> TranslateSExpToIrExp(SExp* sExp, SmTranslationContexts& contexts);

} // namespace Citron