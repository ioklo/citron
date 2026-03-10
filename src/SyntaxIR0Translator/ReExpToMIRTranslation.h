#pragma once
#include <expected>
#include <memory>
#include "MIR/MCreate.h"
#include "MIR/MRead.h"
#include "ReExp.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class IDesignatedDiagnostic;
struct TranslationContexts;

std::expected<MCreate, DiagPtr> TranslateReExpToMCreate(ReExp& reExp, TranslationContexts& contexts);
std::expected<MRead, DiagPtr> TranslateReExpToMRead(ReExp& reExp, TranslationContexts& contexts);
std::expected<MLoc*, DiagPtr> TranslateReExpToMLoc(ReExp& reExp, bool bMaterializeExp, IDesignatedDiagnostic* notLocationDiag, TranslationContexts& contexts);

} // namespace Citron
