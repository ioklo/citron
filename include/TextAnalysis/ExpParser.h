#pragma once
#include "TextAnalysisConfig.h"

#include <Syntax/Syntax.h>

#include <optional>
#include <vector>

namespace Citron {

class Lexer;

TEXT_ANALYSIS_API std::optional<ExpSyntax> ParseExp(Lexer* lexer);

// 1. Assignment, Right Assoc
std::optional<ExpSyntax> ParseAssignExp(Lexer* lexer);

// 2. Equality, Left Assoc
std::optional<ExpSyntax> ParseEqualityExp(Lexer* lexer);    

// 3. TestAndTypeTest, LeftAssoc
std::optional<ExpSyntax> ParseTestAndTypeTestExp(Lexer* lexer);

// 4. Additive, LeftAssoc
std::optional<ExpSyntax> ParseAdditiveExp(Lexer* lexer);

// 5. Multiplicative, LeftAssoc
std::optional<ExpSyntax> ParseMultiplicativeExp(Lexer* lexer);

// 6. Unary, Prefix Inc / Dec
std::optional<ExpSyntax> ParseUnaryExp(Lexer* lexer);

// 7. Primary, Postfix Inc / Dec
std::optional<ExpSyntax> ParsePrimaryExp(Lexer* lexer);

// 8. Single
std::optional<ExpSyntax> ParseSingleExp(Lexer* lexer);

// 기타
std::optional<BoxExpSyntax> ParseBoxExp(Lexer* lexer);
std::optional<NewExpSyntax> ParseNewExp(Lexer* lexer);

// LambdaExpression, Right Assoc
std::optional<LambdaExpSyntax> ParseLambdaExp(Lexer* lexer);
std::optional<ExpSyntax> ParseParenExp(Lexer* lexer);
std::optional<NullLiteralExpSyntax> ParseNullLiteralExp(Lexer* lexer);
std::optional<BoolLiteralExpSyntax> ParseBoolLiteralExp(Lexer* lexer);
std::optional<IntLiteralExpSyntax> ParseIntLiteralExp(Lexer* lexer);
std::optional<StringExpSyntax> ParseStringExp(Lexer* lexer);
std::optional<ListExpSyntax> ParseListExp(Lexer* lexer);
std::optional<IdentifierExpSyntax> ParseIdentifierExp(Lexer* lexer);

std::optional<std::vector<ArgumentSyntax>> ParseCallArgs(Lexer* lexer);

}