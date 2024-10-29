#pragma once

#include <memory>

namespace Citron {

using NLocPtr = std::shared_ptr<class NLoc>;

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

NLocPtr TranslateReThisVarExpToNLoc(ReExp_ThisVar& reExp, TranslationContext& context); // nothrow
NLocPtr TranslateReClassMemberVarExpToNLoc(ReExp_ClassMemberVar& reExp, TranslationContext& context);
NLocPtr TranslateReLocalVarExpToNLoc(ReExp_LocalVar& reExp);
NLocPtr TranslateReLambdaMemberVarExpToNLoc(ReExp_LambdaMemberVar& reExp);
NLocPtr TranslateReStructMemberVarExpToNLoc(ReExp_StructMemberVar& reExp, TranslationContext& context);
NLocPtr TranslateReEnumElemMemberVarExpToNLoc(ReExp_EnumElemMemberVar& reExp, TranslationContext& context);
NLocPtr TranslateReListIndexerExpToNLoc(ReExp_ListIndexer& reExp, TranslationContext& context);
NLocPtr TranslateReLocalDerefExpToNLoc(ReExp_LocalDeref& reExp, TranslationContext& context);
NLocPtr TranslateReBoxDerefExpToNLoc(ReExp_BoxDeref& reExp, TranslationContext& context);

NLocPtr TranslateReExpToNLoc(ReExp& reExp, bool bWrapExpAsLoc, IDesignatedErrorLogger* notLocationErrorLogger, TranslationContext& context);


} // namespace SyntaxIR0Translator
} // namespace Citron