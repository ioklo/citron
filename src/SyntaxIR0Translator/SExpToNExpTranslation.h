#pragma once

#include <memory>
#include <optional>
#include <variant>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron { 

class RType;
using RTypePtr = std::shared_ptr<RType>;

class NExp;
using NExpPtr = std::shared_ptr<NExp>;
class NExp_String;
enum class NInternalUnaryAssignOperator;

namespace SyntaxIR0Translator {

class TranslationContext;

std::expected<NExpPtr, DiagPtr> TranslateSNullLiteralExpToNExp(SExp_NullLiteral& exp, const RTypePtr& hintType, TranslationContext& context);
std::expected<NExpPtr, DiagPtr> TranslateSBoolLiteralExpToNExp(SExp_BoolLiteral& exp);
std::expected<NExpPtr, DiagPtr> TranslateSIntLiteralExpToNExp(SExp_IntLiteral& exp);
std::expected<std::shared_ptr<NExp_String>, DiagPtr> TranslateSStringExpToNStringExp(SExp_String& exp, TranslationContext& context);
std::expected<NExpPtr, DiagPtr> TranslateSIntUnaryAssignExpToNExp(SExp& operand, NInternalUnaryAssignOperator op, TranslationContext& context);
std::expected<NExpPtr, DiagPtr> TranslateSUnaryOpExpToNExpExceptDeref(SExp_UnaryOp& sExp, TranslationContext& context);
std::expected<NExpPtr, DiagPtr> TranslateSAssignBinaryOpExpToNExp(SExp_BinaryOp& exp, TranslationContext& context);
std::expected<NExpPtr, DiagPtr> TranslateSBinaryOpExpToNExp(SExp_BinaryOp& exp, TranslationContext& context);
std::expected<NExpPtr, DiagPtr> TranslateSLambdaExpToNExp(SExp_Lambda& sExp, TranslationContext& context);
std::expected<NExpPtr, DiagPtr> TranslateSListExpToNExp(SExp_List& exp, TranslationContext& context);
std::expected<NExpPtr, DiagPtr> TranslateSNewExpToNExp(SExp_New& exp, TranslationContext& context); // throws ErrorCodeException
std::expected<NExpPtr, DiagPtr> TranslateSCallExpToNExp(SExp_Call& exp, const RTypePtr& hintType, TranslationContext& context);
std::expected<NExpPtr, DiagPtr> TranslateSBoxExpToNExp(SExp_Box& exp, const RTypePtr& hintType, TranslationContext& context);
std::expected<NExpPtr, DiagPtr> TranslateSIsExpToNExp(SExp_Is& exp, TranslationContext& context);
std::expected<NExpPtr, DiagPtr> TranslateSAsExpToNExp(SExp_As& exp, TranslationContext& context);

std::expected<NExpPtr, DiagPtr> TranslateSExpToNExp(SExp& exp, const RTypePtr& hintType, TranslationContext& context);

} // namespace SyntaxIR0Translator
} // namespace Citron

