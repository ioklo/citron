#pragma once

#include <memory>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron { 

class RType;

class NLoc;

namespace SyntaxIR0Translator {

class TranslationContext;
class IDesignatedDiagnostic;

std::expected<NLoc*, DiagPtr> TranslateSExpToNLoc(SExp& sExp, RType* hintType, bool bWrapExpAsLoc, IDesignatedDiagnostic* notLocationDiag, TranslationContext& context);

} // namespace SyntaxIR0Translator 
} // namespace Citron