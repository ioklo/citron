#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"

namespace Citron {

class NLoc;
using NLocPtr = std::shared_ptr<NLoc>;

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

std::expected<NLocPtr, DiagPtr> TranslateReThisVarExpToNLoc(ReExp_ThisVar& reExp, TranslationContext& context); // nothrow
std::expected<NLocPtr, DiagPtr> TranslateReClassVarExpToNLoc(ReExp_ClassVar& reExp, TranslationContext& context);
std::expected<NLocPtr, DiagPtr> TranslateReLocalVarExpToNLoc(ReExp_LocalVar& reExp);
std::expected<NLocPtr, DiagPtr> TranslateReLambdaVarExpToNLoc(ReExp_LambdaVar& reExp);
std::expected<NLocPtr, DiagPtr> TranslateReStructVarExpToNLoc(ReExp_StructVar& reExp, TranslationContext& context);
std::expected<NLocPtr, DiagPtr> TranslateReEnumElemVarExpToNLoc(ReExp_EnumElemVar& reExp, TranslationContext& context);
std::expected<NLocPtr, DiagPtr> TranslateReListIndexerExpToNLoc(ReExp_ListIndexer& reExp, TranslationContext& context);
std::expected<NLocPtr, DiagPtr> TranslateReLocalDerefExpToNLoc(ReExp_LocalDeref& reExp, TranslationContext& context);
std::expected<NLocPtr, DiagPtr> TranslateReBoxDerefExpToNLoc(ReExp_BoxDeref& reExp, TranslationContext& context);

std::expected<NLocPtr, DiagPtr> TranslateReExpToNLoc(ReExp& reExp, bool bWrapExpAsLoc, IDesignatedDiagnostic* notLocationDiag, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron
