#pragma once

#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

struct MLoc;

struct IrExp;
struct IrExp_ClassVar;
struct IrExp_SharedStructVar;
struct IrExp_StructVar;
struct TranslationContexts;

MLoc* TranslateIrExp_ClassVarToMLoc(IrExp_ClassVar* irExp, TranslationContexts& contexts);
MLoc* TranslateIrExp_SharedStructVarToMLoc(IrExp_SharedStructVar* irExp, TranslationContexts& contexts);
std::expected<MLoc*, DiagPtr> TranslateIrExp_StructVarToMLoc(IrExp_StructVar* irExp, TranslationContexts& contexts);

std::expected<MLoc*, DiagPtr> TranslateIrExpToMLoc(IrExp* irExp, TranslationContexts& contexts);

} // namespace 
