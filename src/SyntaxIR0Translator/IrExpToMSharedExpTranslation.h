#pragma once

#include <expected>
#include <memory>

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

class MSharedExp;

struct IrExp;
class IrExp_ClassVar;
class IrExp_SharedStructVar;
class IrExp_StructVar;
struct TranslationContexts;

MSharedExp* TranslateIrExp_ClassVarToMSharedExp(IrExp_ClassVar* irExp, TranslationContexts& contexts);
MSharedExp* TranslateIrExp_SharedStructVarToMSharedExp(IrExp_SharedStructVar* irExp, TranslationContexts& contexts);
std::expected<MSharedExp*, DiagPtr> TranslateIrExp_StructVarToMSharedExp(IrExp_StructVar* irExp, TranslationContexts& contexts);

std::expected<MSharedExp*, DiagPtr> TranslateIrExpToMSharedExp(IrExp* irExp, TranslationContexts& contexts);

} // namespace 