#pragma once

#include <memory>
#include <optional>
#include <variant>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron { 

class RType;

class NExp;
class NExp_String;
enum class NInternalUnaryAssignOperator;

namespace SyntaxIR0Translator {

class TranslationContext;

std::expected<NExp*, DiagPtr> TranslateSNullLiteralExpToNExp(SExp_NullLiteral& exp, RType* hintType, TranslationContext& context);
std::expected<NExp*, DiagPtr> TranslateSBoolLiteralExpToNExp(SExp_BoolLiteral& exp);
std::expected<NExp*, DiagPtr> TranslateSIntLiteralExpToNExp(SExp_IntLiteral& exp);
std::expected<NExp_String*, DiagPtr> TranslateSStringExpToNStringExp(SExp_String& exp, TranslationContext& context);
std::expected<NExp*, DiagPtr> TranslateSIntUnaryAssignExpToNExp(SExp& operand, NInternalUnaryAssignOperator op, TranslationContext& context);
std::expected<NExp*, DiagPtr> TranslateSUnaryOpExpToNExpExceptDeref(SExp_UnaryOp& sExp, TranslationContext& context);
std::expected<NExp*, DiagPtr> TranslateSAssignBinaryOpExpToNExp(SExp_BinaryOp& exp, TranslationContext& context);
std::expected<NExp*, DiagPtr> TranslateSBinaryOpExpToNExp(SExp_BinaryOp& exp, TranslationContext& context);
std::expected<NExp*, DiagPtr> TranslateSLambdaExpToNExp(SExp_Lambda& sExp, TranslationContext& context);
std::expected<NExp*, DiagPtr> TranslateSListExpToNExp(SExp_List& exp, TranslationContext& context);
std::expected<NExp*, DiagPtr> TranslateSNewExpToNExp(SExp_New& exp, TranslationContext& context); // throws ErrorCodeException
std::expected<NExp*, DiagPtr> TranslateSCallExpToNExp(SExp_Call& exp, RType* hintType, TranslationContext& context);
std::expected<NExp*, DiagPtr> TranslateSBoxExpToNExp(SExp_Box& exp, RType* hintType, TranslationContext& context);
std::expected<NExp*, DiagPtr> TranslateSIsExpToNExp(SExp_Is& exp, TranslationContext& context);
std::expected<NExp*, DiagPtr> TranslateSAsExpToNExp(SExp_As& exp, TranslationContext& context);

std::expected<NExp*, DiagPtr> TranslateSExpToNExp(SExp& exp, RType* hintType, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron

