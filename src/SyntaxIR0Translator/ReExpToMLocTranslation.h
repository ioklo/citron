#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"

namespace Citron {

class MLoc;

class IDesignatedDiagnostic;

class ReExp;
class ReExp_ThisVar;
class ReExp_ClassVar;
class ReExp_LocalVar;
class ReExp_LocalRef;
class ReExp_LambdaVar;
class ReExp_StructVar;
class ReExp_EnumElemVar;
class ReExp_ListIndexer;
class ReExp_PtrDeref;
class ReExp_BoxDeref;
struct TranslationContexts;

std::expected<MLoc*, DiagPtr> TranslateReThisVarExpToMLoc(ReExp_ThisVar* reExp, TranslationContexts& contexts); // nothrow
std::expected<MLoc*, DiagPtr> TranslateReClassVarExpToMLoc(ReExp_ClassVar* reExp, TranslationContexts& contexts);
std::expected<MLoc*, DiagPtr> TranslateReLocalVarExpToMLoc(ReExp_LocalVar* reExp, TranslationContexts& contexts);
std::expected<MLoc*, DiagPtr> TranslateReLocalRefExpToMLoc(ReExp_LocalRef* reExp, TranslationContexts& contexts);
std::expected<MLoc*, DiagPtr> TranslateReLambdaVarExpToMLoc(ReExp_LambdaVar* reExp, TranslationContexts& contexts);
std::expected<MLoc*, DiagPtr> TranslateReStructVarExpToMLoc(ReExp_StructVar* reExp, TranslationContexts& contexts);
std::expected<MLoc*, DiagPtr> TranslateReEnumElemVarExpToMLoc(ReExp_EnumElemVar* reExp, TranslationContexts& contexts);
std::expected<MLoc*, DiagPtr> TranslateReListIndexerExpToMLoc(ReExp_ListIndexer* reExp, TranslationContexts& contexts);
std::expected<MLoc*, DiagPtr> TranslateReDerefExpToMLoc(ReExp_PtrDeref* reExp, TranslationContexts& contexts);
std::expected<MLoc*, DiagPtr> TranslateReBoxDerefExpToMLoc(ReExp_BoxDeref* reExp, TranslationContexts& contexts);

std::expected<MLoc*, DiagPtr> TranslateReExpToMLoc(ReExp* reExp, bool bMaterializeExp, IDesignatedDiagnostic* notLocationDiag, TranslationContexts& contexts);

} // namespace Citron
