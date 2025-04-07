export module Citron.SyntaxIR0Translator:SExpToNLocTranslation;

import <memory>;
import <expected>;

import Citron.Diag;
import Citron.Syntax;
import Citron.RDecls;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;
export class IDesignatedDiagnostic;

export std::expected<NLocPtr, DiagPtr> TranslateSExpToNLoc(SExp& sExp, const RTypePtr& hintType, bool bWrapExpAsLoc, IDesignatedDiagnostic* notLocationDiag, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator 