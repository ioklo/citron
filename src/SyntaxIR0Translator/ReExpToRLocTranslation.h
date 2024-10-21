#pragma once

#include <memory>

namespace Citron {

using RLocPtr = std::shared_ptr<class RLoc>;

class RTypeFactory;

using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

class IDesignatedErrorLogger;

class ReExp;
class ReExp_ThisVar;
class ReExp_ClassMemberVar;
class ReExp_LocalVar;
class ReExp_LambdaMemberVar;
class ReExp_StructMemberVar;
class ReExp_EnumElemMemberVar;
class ReExp_ListIndexer;
class ReExp_LocalDeref;
class ReExp_BoxDeref;

class TranslationContext;

RLocPtr TranslateReThisVarExpToRLoc(ReExp_ThisVar& reExp, TranslationContext& context); // nothrow
RLocPtr TranslateReClassMemberVarExpToRLoc(ReExp_ClassMemberVar& reExp, TranslationContext& context);
RLocPtr TranslateReLocalVarExpToRLoc(ReExp_LocalVar& reExp);
RLocPtr TranslateReLambdaMemberVarExpToRLoc(ReExp_LambdaMemberVar& reExp);
RLocPtr TranslateReStructMemberVarExpToRLoc(ReExp_StructMemberVar& reExp, TranslationContext& context);
RLocPtr TranslateReEnumElemMemberVarExpToRLoc(ReExp_EnumElemMemberVar& reExp, TranslationContext& context);
RLocPtr TranslateReListIndexerExpToRLoc(ReExp_ListIndexer& reExp, TranslationContext& context);
RLocPtr TranslateReLocalDerefExpToRLoc(ReExp_LocalDeref& reExp, TranslationContext& context);
RLocPtr TranslateReBoxDerefExpToRLoc(ReExp_BoxDeref& reExp, TranslationContext& context);

RLocPtr TranslateReExpToRLoc(ReExp& reExp, bool bWrapExpAsLoc, IDesignatedErrorLogger* notLocationErrorLogger, TranslationContext& context);


} // namespace SyntaxIR0Translator
} // namespace Citron