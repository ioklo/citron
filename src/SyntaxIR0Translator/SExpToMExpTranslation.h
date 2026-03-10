//#pragma once
//
//#include <memory>
//#include <optional>
//#include <variant>
//#include <expected>
//
//#include "Logging/Diag.h"
//#include "Syntax/Syntax.h"
//
//namespace Citron { 
//
//class RType;
//
//struct MExp;
//class MExp_String;
//enum class MInternalUnaryAssignOperator;
//struct TranslationContexts;
//
//std::expected<MExp*, DiagPtr> TranslateSNullLiteralExpToMExp(SExp_NullLiteral* exp, RType* hintType, TranslationContexts& contexts);
//std::expected<MExp*, DiagPtr> TranslateSBoolLiteralExpToMExp(SExp_BoolLiteral* exp, TranslationContexts& contexts);
//std::expected<MExp*, DiagPtr> TranslateSIntLiteralExpToMExp(SExp_IntLiteral* exp, TranslationContexts& contexts);
//std::expected<MExp_String*, DiagPtr> TranslateSStringExpToMStringExp(SExp_String* exp, TranslationContexts& contexts);
//std::expected<MExp*, DiagPtr> TranslateSIntUnaryAssignExpToMExp(SExp* operand, MInternalUnaryAssignOperator op, TranslationContexts& contexts);
//std::expected<MExp*, DiagPtr> TranslateSUnaryOpExpToMExpExceptDeref(SExp_UnaryOp* sExp, TranslationContexts& contexts);
//std::expected<MExp*, DiagPtr> TranslateSAssignBinaryOpExpToMExp(SExp_BinaryOp* exp, TranslationContexts& contexts);
//std::expected<MExp*, DiagPtr> TranslateSBinaryOpExpToMExp(SExp_BinaryOp* exp, TranslationContexts& contexts);
//std::expected<MExp*, DiagPtr> TranslateSLambdaExpToMExp(SExp_Lambda* sExp, TranslationContexts& contexts);
//std::expected<MExp*, DiagPtr> TranslateSListExpToMExp(SExp_List* exp, TranslationContexts& contexts);
//std::expected<MExp*, DiagPtr> TranslateSNewExpToMExp(SExp_New* exp, TranslationContexts& contexts); // throws ErrorCodeException
//std::expected<MExp*, DiagPtr> TranslateSCallExpToMExp(SExp_Call* exp, RType* hintType, TranslationContexts& contexts);
//std::expected<MExp*, DiagPtr> TranslateSBoxExpToMExp(SExp_Box* exp, RType* hintType, TranslationContexts& contexts);
//std::expected<MExp*, DiagPtr> TranslateSIsExpToMExp(SExp_Is* exp, TranslationContexts& contexts);
//std::expected<MExp*, DiagPtr> TranslateSAsExpToMExp(SExp_As* exp, TranslationContexts& contexts);
//
//std::expected<MExp*, DiagPtr> TranslateSExpToMExp(SExp* exp, RType* hintType, TranslationContexts& contexts);
//
//} // namespace Citron
//
