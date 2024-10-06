#pragma once

#include <memory>

namespace Citron {

class RLoc;
using RLocPtr = std::shared_ptr<RLoc>;

class RTypeFactory;

class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

namespace SyntaxIR0Translator {

class INotLocationErrorLogger;

class ReExp;
class ReThisVarExp;
class ReClassMemberVarExp;
class ReLocalVarExp;
class ReLambdaMemberVarExp;
class ReStructMemberVarExp;
class ReEnumElemMemberVarExp;
class ReListIndexerExp;
class ReLocalDerefExp;
class ReBoxDerefExp;

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

RLocPtr TranslateReThisVarExpToRLoc(ReThisVarExp& reExp); // nothrow
RLocPtr TranslateReClassMemberVarExpToRLoc(ReClassMemberVarExp& reExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RLocPtr TranslateReLocalVarExpToRLoc(ReLocalVarExp& reExp);
RLocPtr TranslateReLambdaMemberVarExpToRLoc(ReLambdaMemberVarExp& reExp);
RLocPtr TranslateReStructMemberVarExpToRLoc(ReStructMemberVarExp& reExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RLocPtr TranslateReEnumElemMemberVarExpToRLoc(ReEnumElemMemberVarExp& reExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RLocPtr TranslateReListIndexerExpToRLoc(ReListIndexerExp& reExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RLocPtr TranslateReLocalDerefExpToRLoc(ReLocalDerefExp& reExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RLocPtr TranslateReBoxDerefExpToRLoc(ReBoxDerefExp& reExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);

RLocPtr TranslateReExpToRLoc(ReExp& reExp, bool bWrapExpAsLoc, INotLocationErrorLogger* notLocationErrorLogger, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);


} // namespace SyntaxIR0Translator
} // namespace Citron