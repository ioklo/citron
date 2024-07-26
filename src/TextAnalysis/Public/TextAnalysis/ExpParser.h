#pragma once
#include "TextAnalysisConfig.h"

#include <Syntax/Syntax.h>

#include <optional>
#include <vector>

namespace Citron {

class Lexer;

TEXTANALYSIS_API SExpPtr ParseExp(Lexer* lexer);

// 1. Assignment, Right Assoc
SExpPtr ParseAssignExp(Lexer* lexer);

// 2. Equality, Left Assoc
SExpPtr ParseEqualityExp(Lexer* lexer);    

// 3. TestAndTypeTest, LeftAssoc
SExpPtr ParseTestAndTypeTestExp(Lexer* lexer);

// 4. Additive, LeftAssoc
SExpPtr ParseAdditiveExp(Lexer* lexer);

// 5. Multiplicative, LeftAssoc
SExpPtr ParseMultiplicativeExp(Lexer* lexer);

// 6. Unary, Prefix Inc / Dec
SExpPtr ParseUnaryExp(Lexer* lexer);

// 7. Primary, Postfix Inc / Dec
SExpPtr ParsePrimaryExp(Lexer* lexer);

// 8. Single
SExpPtr ParseSingleExp(Lexer* lexer);

// 기타
std::unique_ptr<SBoxExp> ParseBoxExp(Lexer* lexer);
std::unique_ptr<SNewExp> ParseNewExp(Lexer* lexer);

// LambdaExpression, Right Assoc
std::unique_ptr<SLambdaExp> ParseLambdaExp(Lexer* lexer);
SExpPtr ParseParenExp(Lexer* lexer);
std::unique_ptr<SNullLiteralExp> ParseNullLiteralExp(Lexer* lexer);
std::unique_ptr<SBoolLiteralExp> ParseBoolLiteralExp(Lexer* lexer);
std::unique_ptr<SIntLiteralExp> ParseIntLiteralExp(Lexer* lexer);
std::unique_ptr<SStringExp> ParseStringExp(Lexer* lexer);
std::unique_ptr<SListExp> ParseListExp(Lexer* lexer);
std::unique_ptr<SIdentifierExp> ParseIdentifierExp(Lexer* lexer);

std::optional<std::vector<SArgument>> ParseCallArgs(Lexer* lexer);

}