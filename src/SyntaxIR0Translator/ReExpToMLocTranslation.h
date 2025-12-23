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
class ReExp_LambdaVar;
class ReExp_StructVar;
class ReExp_EnumElemVar;
class ReExp_ListIndexer;
class ReExp_Deref;
class ReExp_BoxDeref;

class TranslationContext;

std::expected<MLoc*, DiagPtr> TranslateReThisVarExpToMLoc(ReExp_ThisVar* reExp, TranslationContext& context); // nothrow
std::expected<MLoc*, DiagPtr> TranslateReClassVarExpToMLoc(ReExp_ClassVar* reExp, TranslationContext& context);
std::expected<MLoc*, DiagPtr> TranslateReLocalVarExpToMLoc(ReExp_LocalVar* reExp, TranslationContext& context);
std::expected<MLoc*, DiagPtr> TranslateReLambdaVarExpToMLoc(ReExp_LambdaVar* reExp, TranslationContext& context);
std::expected<MLoc*, DiagPtr> TranslateReStructVarExpToMLoc(ReExp_StructVar* reExp, TranslationContext& context);
std::expected<MLoc*, DiagPtr> TranslateReEnumElemVarExpToMLoc(ReExp_EnumElemVar* reExp, TranslationContext& context);
std::expected<MLoc*, DiagPtr> TranslateReListIndexerExpToMLoc(ReExp_ListIndexer* reExp, TranslationContext& context);
std::expected<MLoc*, DiagPtr> TranslateReDerefExpToMLoc(ReExp_Deref* reExp, TranslationContext& context);
std::expected<MLoc*, DiagPtr> TranslateReBoxDerefExpToMLoc(ReExp_BoxDeref* reExp, TranslationContext& context);

std::expected<MLoc*, DiagPtr> TranslateReExpToMLoc(ReExp* reExp, bool bWrapExpAsLoc, IDesignatedDiagnostic* notLocationDiag, TranslationContext& context);

} // namespace Citron
