#pragma once
#include <expected>
#include <memory>
#include "MIR/MCreate.h"
#include "MIR/MRead.h"

namespace Citron { 

using DiagPtr = std::shared_ptr<struct Diag>;
class RType;
class SExp;
struct MLoc;
struct MSharedExp;
struct TranslationContexts;
class IDesignatedDiagnostic;

// 각종 유틸리티 translation을 넣는다

std::expected<MCreate, DiagPtr> TranslateSExpToMCreate(SExp* sExp, RType* hintType, TranslationContexts& contexts);
std::expected<MRead, DiagPtr> TranslateSExpToMRead(SExp* sExp, RType* hintType, TranslationContexts& contexts);
std::expected<MLoc*, DiagPtr> TranslateSExpToMLoc(SExp* sExp, RType* hintType, bool bMaterializeExp, IDesignatedDiagnostic* notLocationDiag, TranslationContexts& contexts);

std::expected<MSharedExp*, DiagPtr> TranslateSExpToMSharedExp(SExp* sExp, TranslationContexts& contexts);



}// namespace Citron

