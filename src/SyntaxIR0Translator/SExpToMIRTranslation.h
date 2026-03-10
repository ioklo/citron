#pragma once

#include <expected>
#include <memory>
#include "MIR/MCreate.h"
#include "MIR/MRead.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class IDesignatedDiagnostic;
class SExp;
class MSharedExp;

struct TranslationContexts;

std::expected<MCreate, DiagPtr> TranslateSExpToMCreate(SExp* sExp, RType* hintType, TranslationContexts& contexts);
std::expected<MRead, DiagPtr> TranslateSExpToMRead(SExp* exp, RType* hintType, TranslationContexts& contexts);
std::expected<MLoc*, DiagPtr> TranslateSExpToMLoc(SExp* sExp, RType* hintType, bool bMaterializeExp, IDesignatedDiagnostic* notLocationDiag, TranslationContexts& contexts);

std::expected<MSharedExp*, DiagPtr> TranslateSExpToMSharedExp(SExp* sExp, TranslationContexts& contexts);

} // namespace Citron