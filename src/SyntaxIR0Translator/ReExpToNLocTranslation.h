#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"

namespace Citron {

class NLoc;

namespace SyntaxIR0Translator {

class IDesignatedDiagnostic;

class ReExp;
class ReExp_ThisVar;
class ReExp_ClassVar;
class ReExp_LocalVar;
class ReExp_LambdaVar;
class ReExp_StructVar;
class ReExp_EnumElemVar;
class ReExp_ListIndexer;
class ReExp_LocalDeref;
class ReExp_BoxDeref;

class TranslationContext;

std::expected<NLoc*, DiagPtr> TranslateReThisVarExpToNLoc(ReExp_ThisVar* reExp, TranslationContext& context); // nothrow
std::expected<NLoc*, DiagPtr> TranslateReClassVarExpToNLoc(ReExp_ClassVar* reExp, TranslationContext& context);
std::expected<NLoc*, DiagPtr> TranslateReLocalVarExpToNLoc(ReExp_LocalVar* reExp, TranslationContext& context);
std::expected<NLoc*, DiagPtr> TranslateReLambdaVarExpToNLoc(ReExp_LambdaVar* reExp, TranslationContext& context);
std::expected<NLoc*, DiagPtr> TranslateReStructVarExpToNLoc(ReExp_StructVar* reExp, TranslationContext& context);
std::expected<NLoc*, DiagPtr> TranslateReEnumElemVarExpToNLoc(ReExp_EnumElemVar* reExp, TranslationContext& context);
std::expected<NLoc*, DiagPtr> TranslateReListIndexerExpToNLoc(ReExp_ListIndexer* reExp, TranslationContext& context);
std::expected<NLoc*, DiagPtr> TranslateReLocalDerefExpToNLoc(ReExp_LocalDeref* reExp, TranslationContext& context);
std::expected<NLoc*, DiagPtr> TranslateReBoxDerefExpToNLoc(ReExp_BoxDeref* reExp, TranslationContext& context);

std::expected<NLoc*, DiagPtr> TranslateReExpToNLoc(ReExp* reExp, bool bWrapExpAsLoc, IDesignatedDiagnostic* notLocationDiag, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron
