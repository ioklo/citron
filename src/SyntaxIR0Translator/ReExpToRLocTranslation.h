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

using ScopeContextPtr = std::shared_ptr<class ScopeContext>;

RLocPtr TranslateReThisVarExpToRLoc(ReExp_ThisVar& reExp, ScopeContext& context, RTypeFactory& factory); // nothrow
RLocPtr TranslateReClassMemberVarExpToRLoc(ReExp_ClassMemberVar& reExp, ScopeContext& context, Logger& logger, RTypeFactory& factory);
RLocPtr TranslateReLocalVarExpToRLoc(ReExp_LocalVar& reExp);
RLocPtr TranslateReLambdaMemberVarExpToRLoc(ReExp_LambdaMemberVar& reExp);
RLocPtr TranslateReStructMemberVarExpToRLoc(ReExp_StructMemberVar& reExp, ScopeContext& context, Logger& logger, RTypeFactory& factory);
RLocPtr TranslateReEnumElemMemberVarExpToRLoc(ReExp_EnumElemMemberVar& reExp, ScopeContext& context, Logger& logger, RTypeFactory& factory);
RLocPtr TranslateReListIndexerExpToRLoc(ReExp_ListIndexer& reExp, ScopeContext& context, Logger& logger, RTypeFactory& factory);
RLocPtr TranslateReLocalDerefExpToRLoc(ReExp_LocalDeref& reExp, ScopeContext& context, Logger& logger, RTypeFactory& factory);
RLocPtr TranslateReBoxDerefExpToRLoc(ReExp_BoxDeref& reExp, ScopeContext& context, Logger& logger, RTypeFactory& factory);

RLocPtr TranslateReExpToRLoc(ReExp& reExp, bool bWrapExpAsLoc, IDesignatedErrorLogger* notLocationErrorLogger, ScopeContext& context, Logger& logger, RTypeFactory& factory);


} // namespace SyntaxIR0Translator
} // namespace Citron