#pragma once
#include <memory>
#include <optional>
#include <variant>

#include <IR0/RExp.h>

namespace Citron {

class SExp;
class SExp_NullLiteral;
class SExp_BoolLiteral;
class SExp_IntLiteral;
using SStringExpElementPtr = std::shared_ptr<class SStringExpElement>;
class SExp_String;
class SExp_UnaryOp;
class SExp_BinaryOp;
class SExp_Lambda;

class SExp_List;
class SExp_New;
class SExp_Call;
class SExp_Box;
class SExp_Is;
class SExp_As;

class RTypeFactory;
class Logger;
using RTypePtr = std::shared_ptr<class RType>;
using LoggerPtr = std::shared_ptr<class Logger>;

namespace SyntaxIR0Translator {

using ScopeContextPtr = std::shared_ptr<class ScopeContext>;

RExpPtr TranslateSNullLiteralExpToRExp(SExp_NullLiteral& exp, const RTypePtr& hintType, const ScopeContextPtr& context, const LoggerPtr& logger);
RExpPtr TranslateSBoolLiteralExpToRExp(SExp_BoolLiteral& exp);
RExpPtr TranslateSIntLiteralExpToRExp(SExp_IntLiteral& exp);
std::shared_ptr<RExp_String> TranslateSStringExpToRStringExp(SExp_String& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RExpPtr TranslateSIntUnaryAssignExpToRExp(SExp& operand, RInternalUnaryAssignOperator op, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RExpPtr TranslateSUnaryOpExpToRExpExceptDeref(SExp_UnaryOp& sExp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RExpPtr TranslateSAssignBinaryOpExpToRExp(SExp_BinaryOp& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RExpPtr TranslateSBinaryOpExpToRExp(SExp_BinaryOp& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RExpPtr TranslateSLambdaExpToRExp(SExp_Lambda& sExp, const LoggerPtr& logger);
RExpPtr TranslateSListExpToRExp(SExp_List& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RExpPtr TranslateSNewExpToRExp(SExp_New& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory); // throws ErrorCodeException
RExpPtr TranslateSCallExpToRExp(SExp_Call& exp, const RTypePtr& hintType, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RExpPtr TranslateSBoxExpToRExp(SExp_Box& exp, const RTypePtr& hintType, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RExpPtr TranslateSIsExpToRExp(SExp_Is& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);
RExpPtr TranslateSAsExpToRExp(SExp_As& exp, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);

RExpPtr TranslateSExpToRExp(SExp& exp, const RTypePtr& hintType, const ScopeContextPtr& context, const LoggerPtr& logger, RTypeFactory& factory);


} // namespace SyntaxIR0Translator
} // namespace Citron
