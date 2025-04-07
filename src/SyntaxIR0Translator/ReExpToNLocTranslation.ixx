export module Citron.SyntaxIR0Translator:ReExpToNLocTranslation;

import <memory>;
import <expected>;

import Citron.Diag;
import Citron.NDecls;


namespace Citron::SyntaxIR0Translator {

export class IDesignatedDiagnostic;

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

export std::expected<NLocPtr, DiagPtr> TranslateReThisVarExpToNLoc(ReExp_ThisVar& reExp, TranslationContext& context); // nothrow
export std::expected<NLocPtr, DiagPtr> TranslateReClassVarExpToNLoc(ReExp_ClassVar& reExp, TranslationContext& context);
export std::expected<NLocPtr, DiagPtr> TranslateReLocalVarExpToNLoc(ReExp_LocalVar& reExp);
export std::expected<NLocPtr, DiagPtr> TranslateReLambdaVarExpToNLoc(ReExp_LambdaVar& reExp);
export std::expected<NLocPtr, DiagPtr> TranslateReStructVarExpToNLoc(ReExp_StructVar& reExp, TranslationContext& context);
export std::expected<NLocPtr, DiagPtr> TranslateReEnumElemVarExpToNLoc(ReExp_EnumElemVar& reExp, TranslationContext& context);
export std::expected<NLocPtr, DiagPtr> TranslateReListIndexerExpToNLoc(ReExp_ListIndexer& reExp, TranslationContext& context);
export std::expected<NLocPtr, DiagPtr> TranslateReLocalDerefExpToNLoc(ReExp_LocalDeref& reExp, TranslationContext& context);
export std::expected<NLocPtr, DiagPtr> TranslateReBoxDerefExpToNLoc(ReExp_BoxDeref& reExp, TranslationContext& context);

export std::expected<NLocPtr, DiagPtr> TranslateReExpToNLoc(ReExp& reExp, bool bWrapExpAsLoc, IDesignatedDiagnostic* notLocationDiag, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator