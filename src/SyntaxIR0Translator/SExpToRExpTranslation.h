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

class TranslationContext;

RExpPtr TranslateSNullLiteralExpToRExp(SExp_NullLiteral& exp, const RTypePtr& hintType, TranslationContext& context);
RExpPtr TranslateSBoolLiteralExpToRExp(SExp_BoolLiteral& exp);
RExpPtr TranslateSIntLiteralExpToRExp(SExp_IntLiteral& exp);
std::shared_ptr<RExp_String> TranslateSStringExpToRStringExp(SExp_String& exp, TranslationContext& context);
RExpPtr TranslateSIntUnaryAssignExpToRExp(SExp& operand, RInternalUnaryAssignOperator op, TranslationContext& context);
RExpPtr TranslateSUnaryOpExpToRExpExceptDeref(SExp_UnaryOp& sExp, TranslationContext& context);
RExpPtr TranslateSAssignBinaryOpExpToRExp(SExp_BinaryOp& exp, TranslationContext& context);
RExpPtr TranslateSBinaryOpExpToRExp(SExp_BinaryOp& exp, TranslationContext& context);
RExpPtr TranslateSLambdaExpToRExp(SExp_Lambda& sExp, TranslationContext& context);
RExpPtr TranslateSListExpToRExp(SExp_List& exp, TranslationContext& context);
RExpPtr TranslateSNewExpToRExp(SExp_New& exp, TranslationContext& context); // throws ErrorCodeException
RExpPtr TranslateSCallExpToRExp(SExp_Call& exp, const RTypePtr& hintType, TranslationContext& context);
RExpPtr TranslateSBoxExpToRExp(SExp_Box& exp, const RTypePtr& hintType, TranslationContext& context);
RExpPtr TranslateSIsExpToRExp(SExp_Is& exp, TranslationContext& context);
RExpPtr TranslateSAsExpToRExp(SExp_As& exp, TranslationContext& context);

RExpPtr TranslateSExpToRExp(SExp& exp, const RTypePtr& hintType, TranslationContext& context);


} // namespace SyntaxIR0Translator
} // namespace Citron
