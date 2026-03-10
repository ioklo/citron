//#pragma once
//
//#include <memory>
//#include <expected>
//
//#include "Logging/Diag.h"
//#include "Syntax/Syntax.h"
//
//namespace Citron { 
//
//class RType;
//struct MLoc;
//class IDesignatedDiagnostic;
//struct TranslationContexts;
//
//std::expected<MLoc*, DiagPtr> TranslateSExpToMLoc(SExp* sExp, RType* hintType, bool bMaterializeExp, IDesignatedDiagnostic* notLocationDiag, TranslationContexts& contexts);
//
//} // namespace Citron