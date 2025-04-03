export module Citron.SyntaxIR0Translator:ReExpToNLocTranslation;

import <memory>;

import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class IDesignatedErrorLogger;

export class ReExp;
export class ReExp_ThisVar;
export class ReExp_ClassVar;
export class ReExp_LocalVar;
export class ReExp_LambdaVar;
export class ReExp_StructVar;
export class ReExp_EnumElemVar;
export class ReExp_ListIndexer;
export class ReExp_LocalDeref;
export class ReExp_BoxDeref;

export class TranslationContext;

export NLocPtr TranslateReThisVarExpToNLoc(ReExp_ThisVar& reExp, TranslationContext& context); // nothrow
export NLocPtr TranslateReClassVarExpToNLoc(ReExp_ClassVar& reExp, TranslationContext& context);
export NLocPtr TranslateReLocalVarExpToNLoc(ReExp_LocalVar& reExp);
export NLocPtr TranslateReLambdaVarExpToNLoc(ReExp_LambdaVar& reExp);
export NLocPtr TranslateReStructVarExpToNLoc(ReExp_StructVar& reExp, TranslationContext& context);
export NLocPtr TranslateReEnumElemVarExpToNLoc(ReExp_EnumElemVar& reExp, TranslationContext& context);
export NLocPtr TranslateReListIndexerExpToNLoc(ReExp_ListIndexer& reExp, TranslationContext& context);
export NLocPtr TranslateReLocalDerefExpToNLoc(ReExp_LocalDeref& reExp, TranslationContext& context);
export NLocPtr TranslateReBoxDerefExpToNLoc(ReExp_BoxDeref& reExp, TranslationContext& context);

export NLocPtr TranslateReExpToNLoc(ReExp& reExp, bool bWrapExpAsLoc, IDesignatedErrorLogger* notLocationErrorLogger, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator