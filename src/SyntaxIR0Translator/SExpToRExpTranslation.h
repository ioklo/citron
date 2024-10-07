#pragma once
#include <memory>
#include <optional>
#include <variant>

#include <IR0/RExp.h>

namespace Citron {

class SExp;
class SNullLiteralExp;
class SBoolLiteralExp;
class SIntLiteralExp;
class SStringExpElement;
using SStringExpElementPtr = std::shared_ptr<SStringExpElement>;
class SStringExp;
class SUnaryOpExp;
class SBinaryOpExp;
class SLambdaExp;

class SListExp;
class SNewExp;
class SCallExp;
class SBoxExp;
class SIsExp;
class SAsExp;

class RType;
using RTypePtr = std::shared_ptr<RType>;

class RTypeFactory;

class Logger;
using LoggerPtr = std::shared_ptr<Logger>;

namespace SyntaxIR0Translator {

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

RExpPtr TranslateSNullLiteralExpToRExp(SNullLiteralExp& exp, const RTypePtr& hintType, const ScopeContextPtr& context, const LoggerPtr& logger);
RExpPtr TranslateSBoolLiteralExpToRExp(SBoolLiteralExp& exp);
RExpPtr TranslateSIntLiteralExpToRExp(SIntLiteralExp& exp);
std::shared_ptr<RStringExp> TranslateSStringExpToRStringExp(SStringExp& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RExpPtr TranslateSIntUnaryAssignExpToRExp(SExp& operand, RInternalUnaryAssignOperator op, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RExpPtr TranslateSUnaryOpExpToRExpExceptDeref(SUnaryOpExp& sExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RExpPtr TranslateSAssignBinaryOpExpToRExp(SBinaryOpExp& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RExpPtr TranslateSBinaryOpExpToRExp(SBinaryOpExp& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RExpPtr TranslateSLambdaExpToRExp(SLambdaExp& sExp, const LoggerPtr& logger);
RExpPtr TranslateSListExpToRExp(SListExp& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RExpPtr TranslateSNewExpToRExp(SNewExp& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory); // throws ErrorCodeException
RExpPtr TranslateSCallExpToRExp(SCallExp& exp, const RTypePtr& hintType, const ScopeContextPtr& context, const LoggerPtr& logger);
RExpPtr TranslateSBoxExpToRExp(SBoxExp& exp, const RTypePtr& hintType, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RExpPtr TranslateSIsExpToRExp(SIsExp& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RExpPtr TranslateSAsExpToRExp(SAsExp& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);

RExpPtr TranslateSExpToRExp(SExp& exp, const RTypePtr& hintType, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);


} // namespace SyntaxIR0Translator
} // namespace Citron
