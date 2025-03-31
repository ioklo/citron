#pragma once

import <memory>;

namespace Citron {

using NLocPtr = std::shared_ptr<class NLoc>;

class RTypeFactory;

using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

class IDesignatedErrorLogger;

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

NLocPtr TranslateReThisVarExpToNLoc(ReExp_ThisVar& reExp, TranslationContext& context); // nothrow
NLocPtr TranslateReClassVarExpToNLoc(ReExp_ClassVar& reExp, TranslationContext& context);
NLocPtr TranslateReLocalVarExpToNLoc(ReExp_LocalVar& reExp);
NLocPtr TranslateReLambdaVarExpToNLoc(ReExp_LambdaVar& reExp);
NLocPtr TranslateReStructVarExpToNLoc(ReExp_StructVar& reExp, TranslationContext& context);
NLocPtr TranslateReEnumElemVarExpToNLoc(ReExp_EnumElemVar& reExp, TranslationContext& context);
NLocPtr TranslateReListIndexerExpToNLoc(ReExp_ListIndexer& reExp, TranslationContext& context);
NLocPtr TranslateReLocalDerefExpToNLoc(ReExp_LocalDeref& reExp, TranslationContext& context);
NLocPtr TranslateReBoxDerefExpToNLoc(ReExp_BoxDeref& reExp, TranslationContext& context);

NLocPtr TranslateReExpToNLoc(ReExp& reExp, bool bWrapExpAsLoc, IDesignatedErrorLogger* notLocationErrorLogger, TranslationContext& context);


} // namespace SyntaxIR0Translator
} // namespace Citron