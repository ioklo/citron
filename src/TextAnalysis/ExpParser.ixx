export module Citron.ExpParser;

import "TextAnalysisConfig.h";

import Citron.Syntax;
import Citron.Lexer;

import <optional>;
import <vector>;

namespace Citron {

export TEXTANALYSIS_API SExpPtr ParseExp(Lexer* lexer);

// 1. Assignment, Right Assoc
export SExpPtr ParseAssignExp(Lexer* lexer);

// 2. Equality, Left Assoc
export SExpPtr ParseEqualityExp(Lexer* lexer);

// 3. TestAndTypeTest, LeftAssoc
export SExpPtr ParseTestAndTypeTestExp(Lexer* lexer);

// 4. Additive, LeftAssoc
export SExpPtr ParseAdditiveExp(Lexer* lexer);

// 5. Multiplicative, LeftAssoc
export SExpPtr ParseMultiplicativeExp(Lexer* lexer);

// 6. Unary, Prefix Inc / Dec
export SExpPtr ParseUnaryExp(Lexer* lexer);

// 7. Primary, Postfix Inc / Dec
export SExpPtr ParsePrimaryExp(Lexer* lexer);

// 8. Single
export SExpPtr ParseSingleExp(Lexer* lexer);

// 기타
export std::shared_ptr<SExp_Box> ParseBoxExp(Lexer* lexer);
export std::shared_ptr<SExp_New> ParseNewExp(Lexer* lexer);

// LambdaExpression, Right Assoc
export std::shared_ptr<SExp_Lambda> ParseLambdaExp(Lexer* lexer);
export SExpPtr ParseParenExp(Lexer* lexer);
export std::shared_ptr<SExp_NullLiteral> ParseNullLiteralExp(Lexer* lexer);
export std::shared_ptr<SExp_BoolLiteral> ParseBoolLiteralExp(Lexer* lexer);
export std::shared_ptr<SExp_IntLiteral> ParseIntLiteralExp(Lexer* lexer);
export std::shared_ptr<SExp_String> ParseStringExp(Lexer* lexer);
export std::shared_ptr<SExp_List> ParseListExp(Lexer* lexer);
export std::shared_ptr<SExp_Identifier> ParseIdentifierExp(Lexer* lexer);

export SArgumentsPtr ParseCallArgs(Lexer* lexer);

}