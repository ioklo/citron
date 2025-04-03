export module Citron.SyntaxIR0Translator:SExpToNLocTranslation;

import <memory>;

import Citron.Syntax;
import Citron.RDecls;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;
export class IDesignatedErrorLogger;

NLocPtr TranslateSExpToNLoc(SExp& sExp, const RTypePtr& hintType, bool bWrapExpAsLoc, IDesignatedErrorLogger* notLocationLogger, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator 