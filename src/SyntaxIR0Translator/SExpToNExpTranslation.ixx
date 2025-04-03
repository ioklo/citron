export module Citron.SyntaxIR0Translator:SExpToNExpTranslation;

import <memory>;
import <optional>;
import <variant>;

import Citron.Syntax;
import Citron.RDecls;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;

export NExpPtr TranslateSNullLiteralExpToNExp(SExp_NullLiteral& exp, const RTypePtr& hintType, TranslationContext& context);
export NExpPtr TranslateSBoolLiteralExpToNExp(SExp_BoolLiteral& exp);
export NExpPtr TranslateSIntLiteralExpToNExp(SExp_IntLiteral& exp);
export std::shared_ptr<NExp_String> TranslateSStringExpToNStringExp(SExp_String& exp, TranslationContext& context);
export NExpPtr TranslateSIntUnaryAssignExpToNExp(SExp& operand, RInternalUnaryAssignOperator op, TranslationContext& context);
export NExpPtr TranslateSUnaryOpExpToNExpExceptDeref(SExp_UnaryOp& sExp, TranslationContext& context);
export NExpPtr TranslateSAssignBinaryOpExpToNExp(SExp_BinaryOp& exp, TranslationContext& context);
export NExpPtr TranslateSBinaryOpExpToNExp(SExp_BinaryOp& exp, TranslationContext& context);
export NExpPtr TranslateSLambdaExpToNExp(SExp_Lambda& sExp, TranslationContext& context);
export NExpPtr TranslateSListExpToNExp(SExp_List& exp, TranslationContext& context);
export NExpPtr TranslateSNewExpToNExp(SExp_New& exp, TranslationContext& context); // throws ErrorCodeException
export NExpPtr TranslateSCallExpToNExp(SExp_Call& exp, const RTypePtr& hintType, TranslationContext& context);
export NExpPtr TranslateSBoxExpToNExp(SExp_Box& exp, const RTypePtr& hintType, TranslationContext& context);
export NExpPtr TranslateSIsExpToNExp(SExp_Is& exp, TranslationContext& context);
export NExpPtr TranslateSAsExpToNExp(SExp_As& exp, TranslationContext& context);

export NExpPtr TranslateSExpToNExp(SExp& exp, const RTypePtr& hintType, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator

