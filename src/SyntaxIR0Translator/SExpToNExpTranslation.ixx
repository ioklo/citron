export module Citron.SyntaxIR0Translator:SExpToNExpTranslation;

import <memory>;
import <optional>;
import <variant>;
import <expected>;

import Citron.Diag;
import Citron.Syntax;
import Citron.RDecls;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;

export std::expected<NExpPtr, DiagPtr> TranslateSNullLiteralExpToNExp(SExp_NullLiteral& exp, const RTypePtr& hintType, TranslationContext& context);
export std::expected<NExpPtr, DiagPtr> TranslateSBoolLiteralExpToNExp(SExp_BoolLiteral& exp);
export std::expected<NExpPtr, DiagPtr> TranslateSIntLiteralExpToNExp(SExp_IntLiteral& exp);
export std::expected<std::shared_ptr<NExp_String>, DiagPtr> TranslateSStringExpToNStringExp(SExp_String& exp, TranslationContext& context);
export std::expected<NExpPtr, DiagPtr> TranslateSIntUnaryAssignExpToNExp(SExp& operand, RInternalUnaryAssignOperator op, TranslationContext& context);
export std::expected<NExpPtr, DiagPtr> TranslateSUnaryOpExpToNExpExceptDeref(SExp_UnaryOp& sExp, TranslationContext& context);
export std::expected<NExpPtr, DiagPtr> TranslateSAssignBinaryOpExpToNExp(SExp_BinaryOp& exp, TranslationContext& context);
export std::expected<NExpPtr, DiagPtr> TranslateSBinaryOpExpToNExp(SExp_BinaryOp& exp, TranslationContext& context);
export std::expected<NExpPtr, DiagPtr> TranslateSLambdaExpToNExp(SExp_Lambda& sExp, TranslationContext& context);
export std::expected<NExpPtr, DiagPtr> TranslateSListExpToNExp(SExp_List& exp, TranslationContext& context);
export std::expected<NExpPtr, DiagPtr> TranslateSNewExpToNExp(SExp_New& exp, TranslationContext& context); // throws ErrorCodeException
export std::expected<NExpPtr, DiagPtr> TranslateSCallExpToNExp(SExp_Call& exp, const RTypePtr& hintType, TranslationContext& context);
export std::expected<NExpPtr, DiagPtr> TranslateSBoxExpToNExp(SExp_Box& exp, const RTypePtr& hintType, TranslationContext& context);
export std::expected<NExpPtr, DiagPtr> TranslateSIsExpToNExp(SExp_Is& exp, TranslationContext& context);
export std::expected<NExpPtr, DiagPtr> TranslateSAsExpToNExp(SExp_As& exp, TranslationContext& context);

export std::expected<NExpPtr, DiagPtr> TranslateSExpToNExp(SExp& exp, const RTypePtr& hintType, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator

