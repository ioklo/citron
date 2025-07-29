#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron { 

class RType;
using RTypePtr = std::shared_ptr<RType>;

class NLoc;
using NLocPtr = std::shared_ptr<NLoc>;

namespace SyntaxIR0Translator {

class TranslationContext;
class IDesignatedDiagnostic;

std::expected<NLocPtr, DiagPtr> TranslateSExpToNLoc(SExp& sExp, const RTypePtr& hintType, bool bWrapExpAsLoc, IDesignatedDiagnostic* notLocationDiag, TranslationContext& context);

} // namespace SyntaxIR0Translator 
} // namespace Citron