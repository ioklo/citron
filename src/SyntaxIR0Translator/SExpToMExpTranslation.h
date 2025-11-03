#pragma once

#include <memory>
#include <optional>
#include <variant>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron { 

class RType;

class MExp;
class MExp_String;
enum class MInternalUnaryAssignOperator;

namespace SyntaxIR0Translator {

class TranslationContext;

std::expected<MExp*, DiagPtr> TranslateSNullLiteralExpToMExp(SExp_NullLiteral* exp, RType* hintType, TranslationContext& context);
std::expected<MExp*, DiagPtr> TranslateSBoolLiteralExpToMExp(SExp_BoolLiteral* exp, TranslationContext& context);
std::expected<MExp*, DiagPtr> TranslateSIntLiteralExpToMExp(SExp_IntLiteral* exp, TranslationContext& context);
std::expected<MExp_String*, DiagPtr> TranslateSStringExpToNStringExp(SExp_String* exp, TranslationContext& context);
std::expected<MExp*, DiagPtr> TranslateSIntUnaryAssignExpToMExp(SExp* operand, MInternalUnaryAssignOperator op, TranslationContext& context);
std::expected<MExp*, DiagPtr> TranslateSUnaryOpExpToMExpExceptDeref(SExp_UnaryOp* sExp, TranslationContext& context);
std::expected<MExp*, DiagPtr> TranslateSAssignBinaryOpExpToMExp(SExp_BinaryOp* exp, TranslationContext& context);
std::expected<MExp*, DiagPtr> TranslateSBinaryOpExpToMExp(SExp_BinaryOp* exp, TranslationContext& context);
std::expected<MExp*, DiagPtr> TranslateSLambdaExpToMExp(SExp_Lambda* sExp, TranslationContext& context);
std::expected<MExp*, DiagPtr> TranslateSListExpToMExp(SExp_List* exp, TranslationContext& context);
std::expected<MExp*, DiagPtr> TranslateSNewExpToMExp(SExp_New* exp, TranslationContext& context); // throws ErrorCodeException
std::expected<MExp*, DiagPtr> TranslateSCallExpToMExp(SExp_Call* exp, RType* hintType, TranslationContext& context);
std::expected<MExp*, DiagPtr> TranslateSBoxExpToMExp(SExp_Box* exp, RType* hintType, TranslationContext& context);
std::expected<MExp*, DiagPtr> TranslateSIsExpToMExp(SExp_Is* exp, TranslationContext& context);
std::expected<MExp*, DiagPtr> TranslateSAsExpToMExp(SExp_As* exp, TranslationContext& context);

std::expected<MExp*, DiagPtr> TranslateSExpToMExp(SExp* exp, RType* hintType, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron

