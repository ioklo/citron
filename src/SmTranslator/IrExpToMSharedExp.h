#pragma once

#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

struct MSharedExp;

struct IrExp;
struct IrExp_ClassVar;
struct IrExp_SharedStructVar;
struct IrExp_StructVar;
struct TranslationContexts;

std::expected<MSharedExp*, DiagPtr> TranslateIrExpToMSharedExp(IrExp* irExp, TranslationContexts& contexts);

} // namespace 