#pragma once
#include <memory>
#include <optional>
#include <variant>

#include <IR0/NExp.h>

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

NExpPtr TranslateSNullLiteralExpToNExp(SExp_NullLiteral& exp, const RTypePtr& hintType, TranslationContext& context);
NExpPtr TranslateSBoolLiteralExpToNExp(SExp_BoolLiteral& exp);
NExpPtr TranslateSIntLiteralExpToNExp(SExp_IntLiteral& exp);
std::shared_ptr<NExp_String> TranslateSStringExpToNStringExp(SExp_String& exp, TranslationContext& context);
NExpPtr TranslateSIntUnaryAssignExpToNExp(SExp& operand, RInternalUnaryAssignOperator op, TranslationContext& context);
NExpPtr TranslateSUnaryOpExpToNExpExceptDeref(SExp_UnaryOp& sExp, TranslationContext& context);
NExpPtr TranslateSAssignBinaryOpExpToNExp(SExp_BinaryOp& exp, TranslationContext& context);
NExpPtr TranslateSBinaryOpExpToNExp(SExp_BinaryOp& exp, TranslationContext& context);
NExpPtr TranslateSLambdaExpToNExp(SExp_Lambda& sExp, TranslationContext& context);
NExpPtr TranslateSListExpToNExp(SExp_List& exp, TranslationContext& context);
NExpPtr TranslateSNewExpToNExp(SExp_New& exp, TranslationContext& context); // throws ErrorCodeException
NExpPtr TranslateSCallExpToNExp(SExp_Call& exp, const RTypePtr& hintType, TranslationContext& context);
NExpPtr TranslateSBoxExpToNExp(SExp_Box& exp, const RTypePtr& hintType, TranslationContext& context);
NExpPtr TranslateSIsExpToNExp(SExp_Is& exp, TranslationContext& context);
NExpPtr TranslateSAsExpToNExp(SExp_As& exp, TranslationContext& context);

NExpPtr TranslateSExpToNExp(SExp& exp, const RTypePtr& hintType, TranslationContext& context);


} // namespace SyntaxIR0Translator
} // namespace Citron
